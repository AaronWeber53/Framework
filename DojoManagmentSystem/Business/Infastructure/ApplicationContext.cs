using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{
    public class ApplicationContext
    {
        private static ApplicationContext _currentApplicationContext { get; set; }

        private static readonly object singleLock = new object();
        
        public Session CurrentSession { get; set; }

        private ApplicationContext()
        {
        }

        public static ApplicationContext CurrentApplicationContext
        {
            get
            {
                if (_currentApplicationContext == null)
                {
                    lock (singleLock)
                    {
                        if (_currentApplicationContext == null)
                        {
                            _currentApplicationContext = new ApplicationContext();
                        }
                    }
                }

                return _currentApplicationContext;
            }
        }
    }
}