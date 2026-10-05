using System;
using System.Text.Encodings.Web;

namespace TaxServiceAndApp.Models
{
    internal class JsonSerializerOptions : Type
    {
        public bool WriteIndented { get; set; }
        public JavaScriptEncoder Encoder { get; set; }
    }
}