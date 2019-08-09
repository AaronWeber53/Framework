using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DojoManagmentSystem.ViewModels
{
    public abstract class ListViewModel
    {
        public ListViewModel()
        {
            var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;

            if (routeValues.ContainsKey("controller"))
                Controller = (string)routeValues["controller"];

            if (routeValues.ContainsKey("action"))
                Action = (string)routeValues["action"];
        }

        public Type ObjectType { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public int? RelationID { get; set; }

        public string DisplayName { get; set; }

        public int CurrentPage { get; set; }

        public string CurrentSort { get; set; }

        public string CurrentSearch { get; set; }

        public int NumberOfPages { get; set; }

        public List<FieldDisplay> FieldsToDisplay { get; set; }

        public bool EnableSearch { get; set; }

        public bool AllowOpen { get; set; } = true;

        public bool ModalOpen { get; set; } = false;

        public bool AllowDelete { get; set; } = true;

        public abstract IHtmlString BuildList();

        public virtual string BuildRowHeaders()
        {
            StringBuilder html = new StringBuilder();
            html.Append("<thead><tr>");

            if (AllowOpen)
            {
                html.Append("<th scope='col'>Open</th>");
            }

            foreach(FieldDisplay column in FieldsToDisplay)
            {
                PropertyInfo field = ObjectType.GetProperty(column.FieldName);
                string headerText = column.HeaderText; 
                if (headerText == null)
                {
                    DisplayNameAttribute displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
                    if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
                    {
                        headerText = displayNameAttribute.DisplayName;
                    }
                    else
                    {
                        headerText = column.FieldName;
                    }
                }
                html.Append("<th scope='col'>");
                html.Append(headerText);
                html.Append("</th>");
            }

            if (AllowDelete)
            {
                html.Append("<th></th>");
            }

            html.Append("</thead></tr>");

            return html.ToString();
        }

        public virtual string BuildRow(BaseModel obj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> classNames = new List<string>();

            stringBuilder.Append($"<tr ");
            if (AllowOpen)
            {
                if (ModalOpen)
                {
                    stringBuilder.Append($"dcata-targeturl='{Controller}/Details/{obj.Id}'");
                    classNames.Add("modal-row-link");
                }
                else
                {
                    stringBuilder.Append($"id='linkRow' data-link='{Controller}/Details/{obj.Id}'");
                }
            }
            stringBuilder.Append("class='");
            foreach(string name in classNames)
            {
                stringBuilder.Append(name + " ");
            }
            stringBuilder.Append("'>");

            if (AllowOpen)
            {
                stringBuilder.Append("<td>");
                if (ModalOpen)
                {
                    stringBuilder.Append($"<button class='btn btn-primary modal-link' data-targeturl='/{Controller}/Details/{obj.Id}'>");
                    stringBuilder.Append("<i class='fas fa-external-link-alt'></i>");
                    stringBuilder.Append("</button>");
                }
                else
                {
                    stringBuilder.Append($"<a class='btn btn-primary' href='/{Controller}/Details/{obj.Id}'>");
                    stringBuilder.Append("<i class='fas fa-external-link-alt'></i>");
                    stringBuilder.Append("</a>");
                }
                stringBuilder.Append("</td>");
            }

            foreach(FieldDisplay column in FieldsToDisplay)
            {
                try
                {
                    PropertyInfo field = ObjectType.GetProperty(column.FieldName);
                    if (field != null)
                    {
                        stringBuilder.Append("<td>");

                        if (field.PropertyType == typeof(bool))
                        {
                            stringBuilder.Append("<div class='form-group'>");
                            stringBuilder.Append("<div class='custom-control custom-checkbox'>");
                            stringBuilder.Append($"<input {((bool)field.GetValue(obj) ? "checked='checked'" : "")} class='custom-control-input' disabled='disabled' id='customCheck1' name='item.HasUser' type='checkbox' value='{field.GetValue(obj)}'>");
                            stringBuilder.Append("<label class='custom-control-label' for='customCheck'></label>");
                            stringBuilder.Append("</div></div>");

                        }
                        else
                        {
                            stringBuilder.Append(field.GetValue(obj));
                        }

                        stringBuilder.Append("</td>");
                    }
                }
                catch
                {
                    throw;
                }
            }

            if (AllowDelete)
            {
                stringBuilder.Append("<td>");
                stringBuilder.Append($"<button type='button' class='btn btn-danger modal-link' data-targeturl='/{Controller}/Delete/{obj.Id}'>");
                stringBuilder.Append("<i class='fas fa-trash-alt'></i>");
                stringBuilder.Append("</button>");
                stringBuilder.Append("</td>");
            }

            stringBuilder.Append("</tr>");

            return stringBuilder.ToString();
        }

        private string BuildPagingButtons(int currentPage, int numberOfPages)
        {
            StringBuilder buttons = new StringBuilder();
            buttons.Append($"Page: {currentPage} of {numberOfPages}<br />");
            buttons.Append("<div>" +
                "<ul class='pagination'>");

            int pageNumber = currentPage <= 1 ? 1 : currentPage - 1;

            if (pageNumber > 1)
            {
                buttons.Append(BuildButton(1));
                buttons.Append("...");
            }

            int counter = 1;

            while (pageNumber <= numberOfPages && counter <= 3)
            {
                buttons.Append(BuildButton(pageNumber, currentPage == pageNumber));
                pageNumber++;
                counter++;
            }

            if (pageNumber <= numberOfPages)
            {
                buttons.Append("...");
                buttons.Append(BuildButton(numberOfPages));
            }


            buttons.Append("</ul>" +
                "</div>");

            return buttons.ToString();
        }

        private string BuildButton(int number, bool isCurrent = false)
        {
            return $"<li class='page-item {(isCurrent ? "disabled" : "")}'><a class='page-link'>{number}</a></li>";
        }
    }

    public class FieldDisplay
    {
        public string HeaderText { get; set; }

        public string FieldName { get; set; }

        public bool AllowSort { get; set; }

        public string SortParam { get; set; }

        public int? FieldWidth { get; set; } = null;
    }

    public class ListViewModel<T> : ListViewModel where T : BaseModel
    {
        public ListViewModel()
        {
            ObjectType = typeof(T);
        }

        public List<T> ObjectList { get; set; }

        public override IHtmlString BuildList()
        {
            StringBuilder html = new StringBuilder();
            html.Append($"<div class='table-responsive' id='pagingList' data-page='{CurrentPage}' data-sort='{CurrentSort}' data-search='{CurrentSearch}' data-baseurl='/{Controller}/List{(RelationID != null ? $"/{RelationID}" : "")}'>");
            html.Append($"<table class='table tabled-hover'>");

            html.Append(BuildRowHeaders());
            html.Append($"<tbody>");

            foreach (BaseModel obj in ObjectList)
            {
                html.Append(BuildRow(obj));
            }

            html.Append($"</tbody>");
            html.Append($"</table>");

            return new MvcHtmlString(html.ToString());
        }
    }
}