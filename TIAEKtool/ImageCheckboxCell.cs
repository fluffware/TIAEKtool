using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TIAEKtool.Properties;

namespace TIAEKtool
{
    public class ImageButtonCell : DataGridViewButtonCell
    {
        static readonly Bitmap[] severity_images = { null, null, Resources.Info, Resources.Warning, Resources.Error};
        protected override void Paint(Graphics graphics,
                Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
                DataGridViewElementStates elementState, object value,
                object formattedValue, string errorText,
                DataGridViewCellStyle cellStyle,
                DataGridViewAdvancedBorderStyle advancedBorderStyle,
                DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            if (!(value is SequentialTask.Severity))
            {
                return;
            }
            int index = (int)value;
            if (index < 0 || index >= severity_images.Count()) return;
            Bitmap image = severity_images[index];
            if (image == null) return;
            Point pos = cellBounds.Location;
            pos.X += (cellBounds.Width - image.Width) / 2;
            pos.Y += (cellBounds.Height - image.Height) / 2;

            graphics.DrawImage(image, new Rectangle(pos, image.Size));
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            return null;
        }
    }
}
