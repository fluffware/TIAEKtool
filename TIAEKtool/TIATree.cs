using System;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.TechnologicalObjects;
using Siemens.Engineering.SW.TechnologicalObjects.Motion;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Cycle;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.Hmi.Globalization;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.Hmi.RuntimeScripting;
using System.Collections.Generic;
using Siemens.Engineering.Online;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Library;
using Siemens.Engineering.Library.Types;
using Siemens.Engineering.Library.MasterCopies;
using Siemens.Engineering.Compare;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;

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
            public TiaPortal TIA;

        
            public Filter Descend = AlwaysTrue; // Examine child items
            public Filter Leaf = AlwaysTrue; // Include this item even if no child item is included

           
            public event EventHandler<BuildDoneEventArgs> BuildDone;
          
            class Handler : NodeHandler
            {
                TreeNodeCollection parent = null;
                TreeNode node = null;
                TreeNodeBuilder builder;
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

            class HandlerAsync : NodeHandler
            {
                Stack<TreeNode> stack = new Stack<TreeNode>();
                
                BackgroundWorker worker;
                TreeNodeBuilder builder;
                private readonly TreeNodeCollection nodes;
                SynchronizationContext caller_ctxt;
                public DoWorkEventArgs WorkerArgs { get; set; }
                public HandlerAsync(TreeNodeBuilder builder, TreeNodeCollection nodes, BackgroundWorker worker, SynchronizationContext caller_ctxt)
                {
                    this.builder = builder;
                    this.nodes = nodes;
                    this.worker = worker;
                    this.caller_ctxt = caller_ctxt;
                }

                class Args
                {
                    public string name;
                    public object obj;
                }
                public override NodeHandler Enter(Object obj, string name)
                {
                    caller_ctxt.Post(EnterHandler, new Args() { name = name, obj = obj });
                    if (worker.CancellationPending)
                    {
                        if (WorkerArgs != null) WorkerArgs.Cancel = true;
                        return null;
                    }
                   
                 
                    if (builder.Descend(obj))
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
                        caller_ctxt.Post(ExitHandler, obj);             
                }

                void EnterHandler(object state)
                {
                    Args args = (Args)state;
                    TreeNode node = new TreeNode(args.name);
                    node.Tag = args.obj;
                    stack.Push(node);
                    Console.WriteLine("Enter: "+node.Text);
                }

                void ExitHandler(object state)
                {
                   
                    TreeNode node = stack.Pop();
                    Console.WriteLine("Exit: " + node.Text);
                  
                    //System.Diagnostics.Debug.Assert(node.Tag == state, node.Text);
                    if (builder.Leaf(state))
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

           

            protected static bool AlwaysTrue(Object obj)
            {
                return true;
            }
            public TreeNodeBuilder(TiaPortal tia)
            {
                TIA = tia;
            }
            public void Build(TreeNodeCollection nodes)
            {
                lock (TIA)
                {
                    Handler handler = new Handler(this, nodes);
                    Handle(handler, TIA);
                }
            }

            protected BackgroundWorker worker;

            public void StartBuild(TreeNodeCollection nodes)
            {
                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += DoWork;
                worker.RunWorkerCompleted += RunWorkerCompleted;

                HandlerAsync handler = new HandlerAsync(this, nodes, worker, SynchronizationContext.Current);
                worker.RunWorkerAsync(handler);
            }

            public void CancelBuild()
            {
                BackgroundWorker w = worker;
                if (w != null)
                {
                    w.CancelAsync();
                    lock (TIA)
                    {
                        // Do nothing just wait for the worker to finish
                    }
                }
                
            }

           


            public void DoWork(object sender, DoWorkEventArgs e)
            {
                lock (TIA)
                {
                    HandlerAsync handler = (HandlerAsync)e.Argument;
                    handler.WorkerArgs = e;
                    Handle(handler, TIA);
                }
            }

            public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                OnBuildDone();
                worker = null;
            }

            protected virtual void OnBuildDone()
            {
                BuildDone(this, new BuildDoneEventArgs());
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

        private static void iterDataBlock(NodeHandler handler, PlcBlockComposition blocks)
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
                iterDataBlock(child_handler, folder.Blocks);
                iterBlockFolder(child_handler, folder.Groups); 
            }
            handler.Exit(folder);
        }

        private static void iterBlockFolder(NodeHandler handler, PlcBlockUserGroupComposition folders)
        {
            foreach (PlcBlockUserGroup folder in folders)
            {    
                HandleBlockFolder(handler, folder);
            }
        }


        // Templates
        private static void HandleScreenTemplate(NodeHandler handler, Siemens.Engineering.Hmi.Screen.ScreenTemplate template)
        {
            handler.Enter(template, template.Name);
            handler.Exit(template);
        }

        private static void iterScreenTemplate(NodeHandler handler,  ScreenTemplateComposition templates)
        {
            foreach (Siemens.Engineering.Hmi.Screen.ScreenTemplate template in templates)
            {
                HandleScreenTemplate(handler, template);
            }
        }

        private static void handleScreenTemplateFolder(NodeHandler handler, ScreenTemplateFolder folder)
        {
            Console.WriteLine("handleScreenTemplateFolder");
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                iterScreenTemplate(child_handler, folder.ScreenTemplates);
                iterScreenTemplateFolder(child_handler, folder.Folders);
            }
            handler.Exit(folder);
        }

        private static void iterScreenTemplateFolder(NodeHandler handler, ScreenTemplateUserFolderComposition folders)
        {
            Console.WriteLine("iterScreenTemplateFolder");
            foreach (ScreenTemplateFolder folder in folders)
            {
                handleScreenTemplateFolder(handler, folder);
            }
        }
        // Popups
        private static void HandleScreenPopup(NodeHandler handler, Siemens.Engineering.Hmi.Screen.ScreenPopup popup)
        {
            handler.Enter(popup, popup.Name);
            handler.Exit(popup);
        }

        private static void iterScreenPopup(NodeHandler handler, ScreenPopupComposition popups)
        {
            foreach (Siemens.Engineering.Hmi.Screen.ScreenPopup popup in popups)
            {
                HandleScreenPopup(handler, popup);
            }
        }

        private static void handleScreenPopupFolder(NodeHandler handler, ScreenPopupFolder folder)
        {
            Console.WriteLine("handleScreenPopupFolder");
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                iterScreenPopup(child_handler, folder.ScreenPopups);
                iterScreenPopupFolder(child_handler, folder.Folders);
            }
            handler.Exit(folder);
        }

        private static void iterScreenPopupFolder(NodeHandler handler, ScreenPopupUserFolderComposition folders)
        {
            Console.WriteLine("iterScreenPopupFolder");
            foreach (ScreenPopupFolder folder in folders)
            {
                handleScreenPopupFolder(handler, folder);
            }
        }

        // Screens

        private static void HandleScreen(NodeHandler handler, Siemens.Engineering.Hmi.Screen.Screen screen)
        {
            handler.Enter(screen, screen.Name);
            handler.Exit(screen);
        }
        private static void iterScreen(NodeHandler handler, ScreenComposition screens)
        {
            foreach (Siemens.Engineering.Hmi.Screen.Screen screen in screens)
            {
                HandleScreen(handler, screen);
            }
        }

        private static void handleScreenFolder(NodeHandler handler, ScreenFolder folder)
        {
            Console.WriteLine("handleScreenFolder");
            NodeHandler child_handler = handler.Enter(folder, folder.Name);
            if (child_handler != null)
            {
                iterScreen(child_handler, folder.Screens);
                iterScreenFolder(child_handler, folder.Folders);
            }
            handler.Exit(folder);
        }

        private static void iterScreenFolder(NodeHandler handler, ScreenUserFolderComposition folders)
        {
            Console.WriteLine("iterScreenFolder");
            foreach (ScreenFolder folder in folders)
            {
                handleScreenFolder(handler, folder);
            }
        }

        // Device items
        private static void handleDeviceItem(NodeHandler handler, DeviceItem item)
        {
            NodeHandler child_handler = handler.Enter(item, item.Name);
            if (child_handler != null)
            {
                SoftwareContainer sw_cont = item.GetService<SoftwareContainer>();
                if (sw_cont != null)
                {
                    PlcSoftware controller = sw_cont.Software as PlcSoftware;
                    if (controller != null)
                    {
                        NodeHandler block_handler = child_handler.Enter(controller.BlockGroup, "Blocks");
                        if (block_handler != null)
                        {
                            iterDataBlock(block_handler, controller.BlockGroup.Blocks);
                            iterBlockFolder(block_handler, controller.BlockGroup.Groups);
                        }
                        child_handler.Exit(controller.BlockGroup);
                    }


                    HmiTarget hmi_target = sw_cont.Software as HmiTarget;
                    if (hmi_target != null)
                    {
                        //Console.WriteLine("HMI target");
                        NodeHandler screen_handler = child_handler.Enter(hmi_target.ScreenFolder, "Screens");
                        if (screen_handler != null)
                        {
                            //Console.WriteLine("Iterating screens");
                            iterScreen(screen_handler, hmi_target.ScreenFolder.Screens);
                            iterScreenFolder(screen_handler, hmi_target.ScreenFolder.Folders);
                        }
                        child_handler.Exit(hmi_target.ScreenFolder);

                        NodeHandler template_handler = child_handler.Enter(hmi_target.ScreenTemplateFolder, "Templates");
                        if (template_handler != null)
                        {
                            //Console.WriteLine("Iterating templates");
                            iterScreenTemplate(template_handler, hmi_target.ScreenTemplateFolder.ScreenTemplates);
                            iterScreenTemplateFolder(template_handler, hmi_target.ScreenTemplateFolder.Folders);
                        }
                        child_handler.Exit(hmi_target.ScreenTemplateFolder);


                        NodeHandler popup_handler = child_handler.Enter(hmi_target.ScreenPopupFolder, "Popups");
                        if (popup_handler != null)
                        {
                            //Console.WriteLine("Iterating popups");
                            iterScreenPopup(template_handler, hmi_target.ScreenPopupFolder.ScreenPopups);
                            iterScreenPopupFolder(template_handler, hmi_target.ScreenPopupFolder.Folders);
                        }
                        child_handler.Exit(hmi_target.ScreenPopupFolder);

                    }
                }
                IterDeviceItem(child_handler, item.DeviceItems);
            }
            handler.Exit(item);
        }
        private static void IterDeviceItem(NodeHandler handler, DeviceItemComposition items)
        {
            foreach (DeviceItem item in items)
            {
                handleDeviceItem(handler, item);
            }
        }

        private static void HandleDevice(NodeHandler handler, Device device)
        {

            NodeHandler child_handler = handler.Enter(device, device.Name);
            if (child_handler != null)
            {
                IterDeviceItem(child_handler, device.DeviceItems);
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

            TreeNode node = new TreeNode(folder.Name);
            node.Tag = folder;
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
        private static void HandleProject(NodeHandler handler, Project proj)
        {
            FileInfo path = proj.Path;
            TreeNode node = new TreeNode(path.Name);
            node.Tag = proj;
            NodeHandler child_handler = handler.Enter(proj, path.Name);
            if (child_handler != null)
            {
                IterDeviceFolder(child_handler, proj.DeviceGroups);
                IterDevice(child_handler, proj.Devices);
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
