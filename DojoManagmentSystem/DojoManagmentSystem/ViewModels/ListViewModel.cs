using Business;
using DojoManagmentSystem.Infastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DojoManagmentSystem.ViewModels
{
    public abstract class ListViewModel
    {
        public Type ObjectType { get; set; }
        public abstract IHtmlString BuildList();
    }

    public class ListViewModel<T> : ListViewModel where T : BaseModel
    {
        public ListViewModel()
        {
            ObjectType = typeof(T);

            var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;

            if (routeValues.ContainsKey("controller"))
                Controller = (string)routeValues["controller"];

            if (routeValues.ContainsKey("action"))
                Action = (string)routeValues["action"];
        }

        public ListSettings ListSettings { get; set; } = new ListSettings();
        public IEnumerable<FieldDisplay> FieldsToDisplay { get; set; }
        public bool ShowArchived { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public long? RelationID { get; set; }
        public string RelationField { get; set; }
        public int CurrentPage { get; set; }
        public Func<T, string> RowClass { get; set; }

        public int NumberOfPages
        {
            get
            {
                return (int)Math.Ceiling((decimal)ObjectList.Count() / (decimal)ListSettings.ItemsPerPage);
            }
        }

        private string _currentSort;
        public string CurrentSort
        {
            get
            {
                if (string.IsNullOrEmpty(_currentSort))
                {
                    _currentSort = "asc";
                }
                return _currentSort;
            }
            set
            {
                _currentSort = value;
            }
        }

        private string Sort
        {
            get
            {
                return CurrentSort == "asc" ? "desc" : "asc";
            }
        }

        public string CurrentSearch { get; set; }

        private string _filterField = null;
        public string FilterField
        {
            get
            {
                if (string.IsNullOrEmpty(_filterField))
                {
                    _filterField = FieldsToDisplay.FirstOrDefault(f => f.AllowSort)?.FieldName;
                }
                return _filterField;
            }
            set
            {
                _filterField = value;

            }
        }

        #region Object List
        private IQueryable<T> _objectList;
        public IQueryable<T> ObjectList
        {
            get
            {
                IQueryable<T> list = _objectList;

                HashSet<string> included = new HashSet<string>();

                foreach (string s in FieldsToDisplay.Where(x => x.FieldName.Contains(".")).Select(x => x.FieldName))
                {
                    string objectPath = s.Substring(0, s.LastIndexOf(".")).Trim();
                    if (!included.Any(x => x == objectPath))
                    {
                        list = list.Include(objectPath);
                        included.Add(objectPath);
                    }
                }

                FieldDisplay filterField = FieldsToDisplay.FirstOrDefault(f => f.FieldName == FilterField);

                if ((filterField?.IsSearchField ?? false) && !string.IsNullOrEmpty(CurrentSearch))
                {
                    list = list.Where($"{filterField.FieldName}=\"{CurrentSearch} \"");
                }

                if (!ShowArchived)
                {
                    list = list.Where(a => !a.IsArchived);
                }
                if (RelationID != null)
                {
                    return list.Where($"{Controller}ID={RelationID}");
                }
                return list;
            }
            set
            {
                _objectList = value;
            }
        }

        private IEnumerable<T> _filteredList;
        public IEnumerable<T> FilteredList
        {
            get
            {
                if (_filteredList == null)
                {
                    IQueryable<T> list = ObjectList;

                    FieldDisplay filterField = FieldsToDisplay.FirstOrDefault(f => f.FieldName == FilterField);

                    if (FilterField != null)
                    {
                        if (CurrentSort == "desc") 
                        {
                            list = list.OrderBy($"{filterField.FieldName} desc");
                        }
                        else
                        {
                            list = list.OrderBy(filterField.FieldName);
                        }
                    }
                    else
                    {
                        list = list.OrderBy(FieldsToDisplay.FirstOrDefault()?.FieldName ?? "");
                    }

                    _filteredList = list.Skip(ListSettings.ItemsPerPage * (CurrentPage - 1)).Take(ListSettings.ItemsPerPage);
                }

                return _filteredList;
            }
        }
        #endregion

        public override IHtmlString BuildList()
        {
            StringBuilder html = new StringBuilder();
            html.Append($"<div id='pagingList' data-filter='{FilterField}' data-page='{CurrentPage}' data-sort='{CurrentSort}' data-search='{CurrentSearch}' data-baseurl='/{Controller}/{Action}{(RelationID != null ? $"/{RelationID}" : "")}'>");

            html.Append(BuildTableHeader());
            html.Append($"<div class='table-responsive'><table class='table tabled-hover'>");
            html.Append(BuildRowHeaders());
            html.Append($"<tbody>");

            foreach (T obj in FilteredList)
            {
                string rowClass = "";
                if (RowClass != null)
                {
                    rowClass = RowClass(obj);
                }
                html.Append(BuildRow(obj, rowClass));
            }

            html.Append($"</tbody>");
            html.Append($"</table></div>");

            html.Append(BuildPagingButtons());
            html.Append("</div>");

            return new MvcHtmlString(html.ToString());
        }

        #region Row
        private string BuildRowHeaders()
        {
            StringBuilder html = new StringBuilder();
            html.Append("<thead><tr>");

            if (ListSettings.AllowOpen)
            {
                html.Append("<th scope='col'>Open</th>");
            }

            foreach (FieldDisplay column in FieldsToDisplay)
            {
                PropertyInfo field = column.GetProperty(ObjectType);
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

                if (column.AllowSort)
                {
                    bool currentFilter = column.FieldName == FilterField;
                    string sort = $"{(currentFilter ? Sort : "asc")}";

                    if (currentFilter)
                    {
                        headerText += CurrentSort == "asc" ? "▼" : "▲";
                    }
                    html.Append($"<a href='' class='sortbutton' data-filter='{column.FieldName}' data-sort='{sort}'>{headerText}</a>");
                }
                else
                {
                    html.Append(headerText);
                }
                html.Append("</th>");
            }

            if (ListSettings.AllowDelete)
            {
                html.Append("<th></th>");
            }

            html.Append("</thead></tr>");

            return html.ToString();
        }

        private string BuildRow(BaseModel obj, params string[] classes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> classNames = new List<string>();
            classNames.AddRange(classes);

            stringBuilder.Append($"<tr ");
            if (ListSettings.AllowOpen)
            {
                if (ListSettings.ModalOpen)
                {
                    stringBuilder.Append($"data-targeturl='/{ObjectType.Name}/Details/{obj.Id}'");
                    classNames.Add("modal-row-link");
                }
                else
                {
                    stringBuilder.Append($"id='linkRow' data-link='/{ObjectType.Name}/Details/{obj.Id}'");
                }
            }
            stringBuilder.Append("class='");
            foreach (string name in classNames)
            {
                stringBuilder.Append(name + " ");
            }

            stringBuilder.Append("'>");

            if (ListSettings.AllowOpen)
            {
                stringBuilder.Append("<td>");
                if (ListSettings.ModalOpen)
                {
                    stringBuilder.Append($"<button class='btn btn-primary modal-link' data-targeturl='/{ObjectType.Name}/Edit/{obj.Id}'>");
                    stringBuilder.Append("<i class='fas fa-external-link-alt'></i>");
                    stringBuilder.Append("</button>");
                }
                else
                {
                    stringBuilder.Append($"<a class='btn btn-primary' href='/{ObjectType.Name}/Details/{obj.Id}'>");
                    stringBuilder.Append("<i class='fas fa-external-link-alt'></i>");
                    stringBuilder.Append("</a>");
                }
                stringBuilder.Append("</td>");
            }

            foreach (FieldDisplay column in FieldsToDisplay)
            {
                    stringBuilder.Append("<td>");

                    if (column.FieldType(obj) == typeof(bool))
                    {
                        stringBuilder.Append("<div class='form-group'>");
                        stringBuilder.Append("<div class='custom-control custom-checkbox'>");
                        stringBuilder.Append($"<input {((bool)column.GetValue(obj) ? "checked='checked'" : "")} class='custom-control-input' disabled='disabled' id='customCheck1' name='item.HasUser' type='checkbox' value='{column.GetValue(obj)}'>");
                        stringBuilder.Append("<label class='custom-control-label' for='customCheck'></label>");
                        stringBuilder.Append("</div></div>");                
                    }
                    else
                    {
                        stringBuilder.Append(column.GetValue(obj));
                    }

                    stringBuilder.Append("</td>");
            }

            if (ListSettings.AllowDelete)
            {
                stringBuilder.Append("<td>");
                stringBuilder.Append($"<button type='button' class='btn btn-danger modal-link' data-targeturl='/{ObjectType.Name}/Delete/{obj.Id}'>");
                stringBuilder.Append("<i class='fas fa-trash-alt'></i>");
                stringBuilder.Append("</button>");
                stringBuilder.Append("</td>");
            }

            stringBuilder.Append("</tr>");

            return stringBuilder.ToString();
        }
        #endregion

        #region Paging
        private string BuildPagingButtons()
        {
            StringBuilder buttons = new StringBuilder();
            buttons.Append("<div class='pagedList'>");
            buttons.Append($"Page: {CurrentPage} of {NumberOfPages}<br />");
            buttons.Append("<div>" +
                "<ul class='pagination'>");

            int pageNumber = CurrentPage <= 1 ? 1 : CurrentPage - 1;

            if (pageNumber > 1)
            {
                buttons.Append(BuildPagingButton(1));
                buttons.Append("...");
            }

            int counter = 1;

            while (pageNumber <= NumberOfPages && counter <= 3)
            {
                buttons.Append(BuildPagingButton(pageNumber, CurrentPage == pageNumber));
                pageNumber++;
                counter++;
            }

            if (pageNumber <= NumberOfPages)
            {
                buttons.Append("...");
                buttons.Append(BuildPagingButton(NumberOfPages));
            }

            buttons.Append("</ul>" +
                "</div></div>");

            return buttons.ToString();
        }

        private string BuildPagingButton(int number, bool isCurrent = false)
        {
            return $"<li class='page-item {(isCurrent ? "disabled" : "")}'><a class='page-link'>{number}</a></li>";
        }
        #endregion

        #region Header Items / Search
        private string BuildSearch()
        {
            StringBuilder html = new StringBuilder();

            html.Append("<div class='form-inline my-2 my-lg-0 float-left'><div><div class='input-group'>");
            FieldDisplay searchField = FieldsToDisplay.FirstOrDefault(f => f.FieldName == FilterField);

            if (searchField != null)
            {
                PropertyInfo field = searchField.GetProperty(ObjectType);
                string headerText = searchField.HeaderText;
                if (headerText == null)
                {
                    DisplayNameAttribute displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
                    if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
                    {
                        headerText = displayNameAttribute.DisplayName;
                    }
                    else
                    {
                        headerText = searchField.FieldName;
                    }
                }

                //TODO: write extension to get field type by only using type instead of entire object. This way don't need to pass in the object in unnessesary places
                html.Append($"<input type='text' name='searchString' class='form-control table-search' placeholder='Search {headerText}' value='{CurrentSearch}' />");
            }

            html.Append("<div class='input-group-append'>");
            html.Append("<button class='btn btn-secondary table-searchbutton' type='button'>");
            html.Append("<i class='fa fa-search'></i></button></div></div></div>");
            html.Append($"<button class='btn btn-secondary my-2 ml-1 table-reset'>Reset</button>");
            html.Append("</div>");

            return html.ToString();
        }

        private string BuildLinkButtons()
        {
            StringBuilder html = new StringBuilder();

            int count = ListSettings.Links.Count;
            foreach(Link link in ListSettings.Links)
            {
                html.Append(link.BuildButton(count <= 1));
                count--;
            }

            return html.ToString();
        }

        private string BuildTableHeader()
        {
            StringBuilder html = new StringBuilder();

            html.Append("<div class='table-header'>");
            if (ListSettings.AllowSearch)
            {
                html.Append(BuildSearch());
            }

            if (ListSettings.Links.Count > 0)
            {
                html.Append("<div class='float-right'>");
                html.Append(BuildLinkButtons());
                html.Append("</div>");
            }

            html.Append("</div>");
            return html.ToString();
        }
        #endregion
    }

    public class ListSettings
    {
        public string DisplayName { get; set; }
        public bool AllowOpen { get; set; } = true;
        public bool ModalOpen { get; set; } = false;
        public bool AllowDelete { get; set; } = true;
        public bool AllowSearch { get; set; } = true;
        public int ItemsPerPage { get; set; } = 5;
        public List<Link> Links { get; set; } = new List<Link>();
    }
}