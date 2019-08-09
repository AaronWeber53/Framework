using DojoManagmentSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DojoManagmentSystem.Infastructure.Extensions
{
    public static class HtmlExtensions
    {
        public static IHtmlString BuildList(this HtmlHelper htmlHelper, ListViewModel list)
        {
            return list.BuildList();
        }

        public static IHtmlString GeneratePagingButtons(this HtmlHelper htmlHelper, ListViewModel viewModel)
        {
            return new MvcHtmlString(BuildPagingButtons(viewModel.CurrentPage, viewModel.NumberOfPages));
        }

        public static IHtmlString GeneratePagingButtons(this HtmlHelper htmlHelper, int currentPage, int numberOfPages)
        {
            return new MvcHtmlString(BuildPagingButtons(currentPage, numberOfPages));
        }

        private static string BuildPagingButtons(int currentPage, int numberOfPages)
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

        private static string BuildButton(int number, bool isCurrent = false)
        {
            return $"<li class='page-item {(isCurrent ? "disabled" : "")}'><a class='page-link'>{number}</a></li>";
        }
    }
}