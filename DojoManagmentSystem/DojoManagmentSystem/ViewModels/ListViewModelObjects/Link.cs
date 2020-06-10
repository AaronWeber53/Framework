using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public class Link
    {
        public string URL { get; set; }
        public string Text { get; set; }
        public bool ModalLink { get; set; } = true;

        public enum Icons
        {
            Print,
            Add,
            Details,
            None
        }
        public Icons Icon { get; set; } = Icons.Add;

        private string GetIconTag()
        {
            StringBuilder html = new StringBuilder();
            html.Append("<i class='fas ");

            switch (Icon)
            {
                case Icons.Print:
                    html.Append("fa-print");
                    break;
                case Icons.Add:
                    html.Append("fa-plus");
                    break;
                case Icons.Details:
                    html.Append("fa-external-link-alt");
                    break;
                case Icons.None:
                default:
                    break;
            }

            html.Append("'></i> ");
            return html.ToString();
        }

        public enum Color
        {
            Green,
            Blue,
            LightBlue
        }
        public Color ButtonColor { get; set; } = Color.Green;

        private string GetColorClass()
        {
            string className = "";
            switch (ButtonColor)
            {
                case Color.Blue:
                    className = "btn-info";
                    break;
                case Color.LightBlue:
                    className = "btn-info";
                    break;
                case Color.Green:
                default:
                    className = "btn-primary";
                    break;
            }
            return className;
        }

        public string BuildButton(bool lastButton)
        {
            StringBuilder html = new StringBuilder();
            if (ModalLink)
            {
                html.Append($"<button type='button' data-targeturl='{URL}' ");
                html.Append($"class='btn btn-sm modal-link {GetColorClass()} {(lastButton ? "" : "mr-2")}'>");
            }
            else
            {
                html.Append($"<a>");
            }

            html.Append(GetIconTag());
            html.Append(Text);
            html.Append(ModalLink ? "</button>" : "</a>");
            return html.ToString();
        }
    }
}