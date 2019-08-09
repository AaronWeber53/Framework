using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.ViewModels
{
    public abstract class ListViewModel
    {
        public int CurrentPage { get; set; }

        public string CurrentSort { get; set; }

        public string CurrentSearch { get; set; }

        public int NumberOfPages { get; set; }
    }

    public class ListViewModel<T> : ListViewModel where T : BaseModel
    {
        public List<T> ObjectList { get; set; }
    }
}