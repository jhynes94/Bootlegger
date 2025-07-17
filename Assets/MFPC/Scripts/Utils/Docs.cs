using System;

namespace MFPC.Utils
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Docs : Attribute
    {
        readonly string desc;

        public Docs(string desc)
        {
            this.desc = desc;
        }

        public string GetDocs()
        {
            return desc;
        }
    }
}