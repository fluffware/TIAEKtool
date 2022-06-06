using System;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Communication;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TIAEKtool;
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.UI.Screens;
using Siemens.Engineering.HmiUnified.UI.Base;
using Siemens.Engineering.HmiUnified.HmiTags;

namespace TIAtool
{
    public class TIATree
    {
        public delegate bool Filter(Object item); // Predicate for item

        // Discards all device items that isn't a controller or a container
        public static bool ControllerOnly(Object obj)
        {
            if (!(obj is DeviceItem)) return true;
            DeviceItem item = (DeviceItem)obj;
            SoftwareContainer cont = item.GetService<SoftwareContainer>();
            if (cont == null) return true;
            return (cont.Software is PlcSoftware);
  
        }

        public static bool BlockGroupOrParent(Object obj)
        {
            if (ControllerOnly(obj)) return true;
            if (!(obj is Siemens.Engineering.SW.Blocks.PlcBlockGroup)) return false;
            return true;
        }

        public static bool BlockOrBlockGroup(Object obj)
        {
            if (!(obj is Siemens.Engineering.SW.Blocks.PlcBlockGroup) && !(obj is Siemens.Engineering.SW.Blocks.PlcBlock)) return false;
            return true;
        }

        public static bool SharedDBOnly(Object obj)
        {
            return (obj is GlobalDB);
        }

        public class BuildDoneEventArgs : EventArgs
        {

        }

        public class TreeNodeBuilder
        {
            public readonly TiaPortal TIA;
            readonly TIAAsyncWrapper thread;

            public Filter Descend = AlwaysTrue; // Examine child items
            public Filter Leaf = AlwaysTrue; // Include this item even if no child item is included

           
            public event EventHandler<BuildDoneEventArgs> BuildDone;
          
            class Handler : NodeHandler
            {
                readonly TreeNodeCollection parent = null;
                TreeNode node = null;
                readonly TreeNodeBuilder builder;
                public Handler(TreeNodeBuilder builder, TreeNodeCollection nodes) {
                    parent = nodes;
                    this.builder = builder;
                }

                public override NodeHandler Enter(Object obj, string name)
                {
                    node = new TreeNode(name)
                    {
                        Tag = obj
                    };
                    if (builder.Descend(obj))
                    {
                        return new Handler(builder, node.Nodes);
                    }
                    else
                    {
                        return null;
                    }
                }

                public override void Exit(Object obj)
                {
                    if (builder.Leaf(obj) || node.Nodes.Count > 0)
                    {
                        parent.Add(node);
                    }
                    node = null;
                }
            }

            

           

            protected static bool AlwaysTrue(Object obj)
            {
                return true;
            }
            public TreeNodeBuilder(TIAAsyncWrapper tiaThread, TiaPortal tia)
            {
                TIA = tia;
                thread = tiaThread;
            }

            class BuildTask : TIAAsyncWrapper.Task
            {
                readonly TiaPortal TIA;
                readonly Stack<TreeNode> stack = new Stack<TreeNode>();
                readonly TreeNodeBuilder builder;
                private readonly TreeNodeCollection nodes;
                readonly HandlerAsync handler;

                class HandlerAsync : NodeHandler
                {
                    readonly BuildTask task;

                    public HandlerAsync(BuildTask task)
                    {
                        this.task = task;
                    }

                    public class EnterArgs
                    {
                        public string name;
                        public object obj;
                    }

                    public override NodeHandler Enter(Object obj, string name)
                    {
                        task.SendResult(new EnterArgs() { name = name, obj = obj });
                        if (task.cancelled)
                        {
                            return null;
                        }


                        if (task.builder.Descend(obj))
                        {
                            return this;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    public override void Exit(object obj)
                    {
                        task.SendResult(obj);
                    }


                }
                
                public BuildTask(TreeNodeBuilder builder, TreeNodeCollection nodes, TiaPortal tia)
                {
                    this.builder = builder;
                    this.nodes = nodes;
                    this.TIA = tia;
                    handler = new HandlerAsync(this);
                }
                public override object Run()
                {
                    lock (TIA)
                    {
                        Handle(handler, TIA);
                    }
                    return null;
                }

                public override void Done(object result)
                {
                    builder.BuildDone(this, new BuildDoneEventArgs());
                }

                public override void Result(object result)
                {
                    if (result is HandlerAsync.EnterArgs enterArgs)
                    {

                        TreeNode node = new TreeNode(enterArgs.name)
                        {
                            Tag = enterArgs.obj
                        };
                        stack.Push(node);
                        Console.WriteLine("Enter: " + node.Text);
                    }


                    else
                    {

                        TreeNode node = stack.Pop();
                        Console.WriteLine("Exit: " + node.Text);

                        //System.Diagnostics.Debug.Assert(node.Tag == state, node.Text);
                        if (builder.Leaf(result))
                        {
                            TreeNode n = node;
                            // Add all nodes on the stack that hasn't already got a parent
                            if (n.Parent == null)
                            {
                                foreach (TreeNode p in stack)
                                {
                                    p.Nodes.Add(n);
                                    if (p.Parent != null)
                                    {
                                        n = null;
                                        break;
                                    }
                                    n = p;
                                }
                                if (n != null && !nodes.Contains(n))
                                {
                                    nodes.Add(n);
                                }
                            }
                        }
                    }
                }

                public override void CaughtException(Exception ex)
                {
                   
                    Console.WriteLine("Failed to build node tree: " + ex.Message +"\n"+ ex.StackTrace);
                }

            }
            public void StartBuild(TreeNodeCollection nodes)
            {
                BuildTask task = new BuildTask(this, nodes,TIA);
                thread.RunAsync(task);
            }

            public void CancelBuild()
            {
                TIAAsyncWrapper t = thread;
                if (t != null)
                {
                    t.cancel();
                    lock (TIA)
                    {
                        // Do nothing just wait for the worker to finish
                    }
                }
                
            }

           


            // Expand all nodes with max_children or fewer
            public static void Expand(TreeNodeCollection nodes, int max_children)
            {

                foreach (TreeNode node in nodes)
                {
                    if (node.Nodes.Count <= max_children) node.Expand();
                    Expand(node.Nodes, max_children);
                }
            }
        }

        public class NodeHandler
        {
            /* Called before children are handled. Returns a handler used for the children, if null the children are ignored */
            public virtual NodeHandler Enter(Object obj, string name)
            {
                return this; // Reuse this handler for the children
            }
            /* Called after the children have been handled */
            public virtual void Exit(Object obj)
            {

            }
        }

        private static void HandleDataBlock(NodeHandler handler, PlcBlock block)
        {
            handler.Enter(block, block.Name);
            handler.Exit(block);
        }

        private static void IterDataBlock(NodeHandler handler, PlcBlockComposition blocks)
        {
            foreach (PlcBlock block in blocks)
            {
                HandleDataBlock(handler, block);
            }
        }
        private static void HandleBlockFolder(NodeHandler handler, PlcBlockGroup folder)
        {
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null) {       
                IterDataBlock(child_handler, folder.Blocks);
                IterBlockFolder(child_handler, folder.Groups); 
            }
            handler.Exit(folder);
        }

        private static void IterBlockFolder(NodeHandler handler, PlcBlockUserGroupComposition folders)
        {
            foreach (PlcBlockUserGroup folder in folders)
            {    
                HandleBlockFolder(handler, folder);
            }
        }

        // PLC types
        private static void HandleType(NodeHandler handler, PlcType type)
        {
            handler.Enter(type, type.Name);
            handler.Exit(type);
        }

        private static void IterType(NodeHandler handler, PlcTypeComposition types)
        {
            foreach (PlcType type in types)
            {
                HandleType(handler, type);
            }
        }
        private static void HandleTypeFolder(NodeHandler handler, PlcTypeUserGroup folder)
        {
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                IterType(child_handler, folder.Types);
                IterTypeFolder(child_handler, folder.Groups);
            }
            handler.Exit(folder);
        }

        private static void IterTypeFolder(NodeHandler handler, PlcTypeUserGroupComposition folders)
        {
            foreach (PlcTypeUserGroup folder in folders)
            {
                HandleTypeFolder(handler, folder);
            }
        }

        // Templates
        private static void HandleScreenTemplate(NodeHandler handler, Siemens.Engineering.Hmi.Screen.ScreenTemplate template)
        {
            handler.Enter(template, template.Name);
            handler.Exit(template);
        }

        private static void IterScreenTemplate(NodeHandler handler,  ScreenTemplateComposition templates)
        {
            foreach (Siemens.Engineering.Hmi.Screen.ScreenTemplate template in templates)
            {
                HandleScreenTemplate(handler, template);
            }
        }

        private static void HandleScreenTemplateFolder(NodeHandler handler, ScreenTemplateFolder folder)
        {
            Console.WriteLine("handleScreenTemplateFolder");
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                IterScreenTemplate(child_handler, folder.ScreenTemplates);
                IterScreenTemplateFolder(child_handler, folder.Folders);
            }
            handler.Exit(folder);
        }

        private static void IterScreenTemplateFolder(NodeHandler handler, ScreenTemplateUserFolderComposition folders)
        {
            Console.WriteLine("iterScreenTemplateFolder");
            foreach (ScreenTemplateFolder folder in folders)
            {
                HandleScreenTemplateFolder(handler, folder);
            }
        }
        // Popups
        private static void HandleScreenPopup(NodeHandler handler, Siemens.Engineering.Hmi.Screen.ScreenPopup popup)
        {
            handler.Enter(popup, popup.Name);
            handler.Exit(popup);
        }

        private static void IterScreenPopup(NodeHandler handler, ScreenPopupComposition popups)
        {
            foreach (Siemens.Engineering.Hmi.Screen.ScreenPopup popup in popups)
            {
                HandleScreenPopup(handler, popup);
            }
        }

        private static void HandleScreenPopupFolder(NodeHandler handler, ScreenPopupFolder folder)
        {
            Console.WriteLine("handleScreenPopupFolder");
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                IterScreenPopup(child_handler, folder.ScreenPopups);
                IterScreenPopupFolder(child_handler, folder.Folders);
            }
            handler.Exit(folder);
        }

        private static void IterScreenPopupFolder(NodeHandler handler, ScreenPopupUserFolderComposition folders)
        {
            Console.WriteLine("iterScreenPopupFolder");
            foreach (ScreenPopupFolder folder in folders)
            {
                HandleScreenPopupFolder(handler, folder);
            }
        }

        // Screens

        private static void HandleScreen(NodeHandler handler, Siemens.Engineering.Hmi.Screen.Screen screen)
        {
            handler.Enter(screen, screen.Name);
            handler.Exit(screen);
        }
        private static void IterScreen(NodeHandler handler, ScreenComposition screens)
        {
            foreach (Siemens.Engineering.Hmi.Screen.Screen screen in screens)
            {
                HandleScreen(handler, screen);
            }
        }

        private static void HandleScreenFolder(NodeHandler handler, ScreenFolder folder)
        {
            Console.WriteLine("handleScreenFolder");
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                IterScreen(child_handler, folder.Screens);
                IterScreenFolder(child_handler, folder.Folders);
            }
            handler.Exit(folder);
        }

        private static void IterScreenFolder(NodeHandler handler, ScreenUserFolderComposition folders)
        {
            Console.WriteLine("iterScreenFolder");
            foreach (ScreenFolder folder in folders)
            {
                HandleScreenFolder(handler, folder);
            }
        }

        private static void IterNetNodes(NodeHandler handler, NodeComposition nodes)
        {
            foreach (Node n in nodes)
            {
                NodeHandler child_handler = handler.Enter(n, n.Name);
                if (child_handler != null)
                {

                }
                handler.Exit(n);
            }
        }

        // Unified screens
        private static void HandleUnifiedScreens(NodeHandler handler, HmiScreenComposition screens)
        {
            NodeHandler screen_handler = handler.Enter(screens, "Screens");
            if (screen_handler != null)
            {
                foreach (HmiScreen screen in screens)
                {
                    var item_handler = screen_handler.Enter(screen, screen.Name);
                    if (item_handler != null)
                    {
                        HmiScreenItemBaseComposition items = screen.ScreenItems;
                        foreach (HmiScreenItemBase screen_item in items)
                        {
                            item_handler.Enter(screen_item, screen_item.Name);
                            item_handler.Exit(screen_item);
                        }
                    }
                    screen_handler.Exit(screen);
                }
            }
            handler.Exit(screens);
        }
        private static void HandleUnifiedTagTables(NodeHandler handler, HmiTagTableComposition tag_tables)
        {
            NodeHandler tables_handler = handler.Enter(tag_tables, "Tag tables");
            if (tables_handler != null)
            {
                foreach (HmiTagTable table in tag_tables) {
                    NodeHandler table_handler = handler.Enter(table, table.Name);
                    if (table_handler != null)
                    {
                        HmiTagComposition tags = table.Tags;
                        foreach (HmiTag tag in tags)
                        {
                            table_handler.Enter(tag, tag.Name);
                            table_handler.Exit(tag);
                        }
                    }
                    handler.Exit(table);
                }
                handler.Exit(tag_tables);
            }
            handler.Exit(tag_tables);
        }

        // Device items
        private static void HandleDeviceItem(NodeHandler handler, DeviceItem item)
        {
            NodeHandler child_handler = handler.Enter(item, item.Name);
            if (child_handler != null)
            {
                SoftwareContainer sw_cont = item.GetService<SoftwareContainer>();
                if (sw_cont != null)
                {
                    if (sw_cont.Software is PlcSoftware controller)
                    {
                        NodeHandler block_handler = child_handler.Enter(controller.BlockGroup, "Blocks");
                        if (block_handler != null)
                        {
                            IterDataBlock(block_handler, controller.BlockGroup.Blocks);
                            IterBlockFolder(block_handler, controller.BlockGroup.Groups);
                        }
                        child_handler.Exit(controller.BlockGroup);
                        NodeHandler type_handler = child_handler.Enter(controller.TypeGroup, "Types");
                        if (type_handler != null)
                        {
                            IterType(type_handler, controller.TypeGroup.Types);
                            IterTypeFolder(block_handler, controller.TypeGroup.Groups);
                        }
                        child_handler.Exit(controller.TypeGroup);
                    }


                    if (sw_cont.Software is HmiTarget hmi_target)
                    {
                        //Console.WriteLine("HMI target");
                        NodeHandler screen_handler = child_handler.Enter(hmi_target.ScreenFolder, "Screens");
                        if (screen_handler != null)
                        {
                            //Console.WriteLine("Iterating screens");
                            IterScreen(screen_handler, hmi_target.ScreenFolder.Screens);
                            IterScreenFolder(screen_handler, hmi_target.ScreenFolder.Folders);
                        }
                        child_handler.Exit(hmi_target.ScreenFolder);

                        NodeHandler template_handler = child_handler.Enter(hmi_target.ScreenTemplateFolder, "Templates");
                        if (template_handler != null)
                        {
                            //Console.WriteLine("Iterating templates");
                            IterScreenTemplate(template_handler, hmi_target.ScreenTemplateFolder.ScreenTemplates);
                            IterScreenTemplateFolder(template_handler, hmi_target.ScreenTemplateFolder.Folders);
                        }
                        child_handler.Exit(hmi_target.ScreenTemplateFolder);


                        NodeHandler popup_handler = child_handler.Enter(hmi_target.ScreenPopupFolder, "Popups");
                        if (popup_handler != null)
                        {
                            //Console.WriteLine("Iterating popups");
                            IterScreenPopup(template_handler, hmi_target.ScreenPopupFolder.ScreenPopups);
                            IterScreenPopupFolder(template_handler, hmi_target.ScreenPopupFolder.Folders);
                        }
                        child_handler.Exit(hmi_target.ScreenPopupFolder);

                        NodeHandler connection_handler = child_handler.Enter(hmi_target.Connections, "Connections");
                        if (connection_handler != null)
                        {
                            foreach (Connection connection in hmi_target.Connections)
                            {
                                connection_handler.Enter(connection, connection.Name);

                                connection_handler.Exit(connection);
                            }
                        }
                        child_handler.Exit(hmi_target.Connections);

                    }
                    if (sw_cont.Software is HmiSoftware hmi_software)
                    {
                        HmiScreenComposition screens = hmi_software.Screens;
                        HandleUnifiedScreens(child_handler, screens);
                        HmiTagTableComposition tags = hmi_software.TagTables;
                        HandleUnifiedTagTables(child_handler, tags);
                    }
                }

                NetworkInterface netif = item.GetService<NetworkInterface>();
                if (netif != null)
                {
                    NodeHandler netif_handler = child_handler.Enter(netif, "Nodes");
                    if (netif_handler != null)
                    {
                        IterNetNodes(netif_handler, netif.Nodes);
                    }
                    child_handler.Exit(netif);
                }
            }
            handler.Exit(item);
        }
        private static void IterDeviceItem(NodeHandler handler, DeviceItemComposition items)
        {
            foreach (DeviceItem item in items)
            {
                HandleDeviceItem(handler, item);
            }
        }

        private static void HandleDevice(NodeHandler handler, Device device)
        {

            NodeHandler child_handler = handler.Enter(device, device.Name);
            if (child_handler != null)
            {
                IterDeviceItem(child_handler, device.DeviceItems);

                NetworkInterface netif = device.GetService<NetworkInterface>();
                if (netif != null)
                {
                    NodeHandler netif_handler = child_handler.Enter(netif, "Nodes");
                    if (netif_handler != null)
                    {
                        IterNetNodes(netif_handler, netif.Nodes);
                    }
                    child_handler.Exit(netif);
                }
            }
            handler.Exit(device);
        }

        private static void IterDevice(NodeHandler handler, DeviceComposition devices)
        {
            foreach (Device d in devices)
            {
                HandleDevice(handler, d);
            }

        }
        private static void HandleDeviceFolder(NodeHandler handler, DeviceUserGroup folder)
        {

            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                IterDeviceFolder(child_handler, folder.Groups);
                IterDevice(child_handler, folder.Devices);
            }
            handler.Exit(folder);
        }
        private static void IterDeviceFolder(NodeHandler handler, DeviceUserGroupComposition folders)
        {
            foreach (DeviceUserGroup f in folders)
            {
                HandleDeviceFolder(handler, f);
            }
        }

        private static void IterNetNodes(NodeHandler handler, NodeAssociation nodes)
        {
            foreach (Node n in nodes)
            {
                NodeHandler child_handler = handler.Enter(n, n.Name);
                if (child_handler != null)
                {
                  
                }
                handler.Exit(n);
            }
        }
        private static void IterSubnets(NodeHandler handler, SubnetComposition subnets)
        {
            foreach (Subnet s in subnets)
            {
                NodeHandler child_handler = handler.Enter(s, s.Name);
                if (child_handler != null)
                {
                    IterNetNodes(child_handler, s.Nodes);
                }
                handler.Exit(s);
            }
        }
        private static void HandleProject(NodeHandler handler, Project proj)
        {
            FileInfo path = proj.Path;
            NodeHandler child_handler = handler.Enter(proj, path.Name);
            if (child_handler != null)
            {
                IterDeviceFolder(child_handler, proj.DeviceGroups);
                IterDevice(child_handler, proj.Devices);

                NodeHandler subnet_handler = child_handler.Enter(proj.Subnets, "Subnets");
                if (subnet_handler != null)
                {
                    IterSubnets(subnet_handler, proj.Subnets);

                }

                child_handler.Exit(proj.Subnets);
            }

            handler.Exit(proj);

           



        }
        public static void Handle(NodeHandler handler, TiaPortal tia)
        {
            foreach (Project p in tia.Projects)
            {
                HandleProject(handler, p);
            }

        }

    }
}
