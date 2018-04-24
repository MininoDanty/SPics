using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SPics.Models
{
    internal class Pic
    {

        internal const string SEPARADOR = ", ";
        public string Name { get; set; }

        public string Path { get; set; }

        public Image Image { get; set; }

        public List<string> Tags { get; set; }
        public string TagsAsString
        {
            get
            {
                if (Tags != null)
                {
                    string result = "";

                    for (int i = 0; i < Tags.Count; i++)
                    {
                        result += Tags[i] + SEPARADOR;
                    }
                    return result.Remove(result.Length - SEPARADOR.Length, SEPARADOR.Length);
                }


                return "Error";
            }
            set
            {
                ;
            }
        }



        public string TagsForUI { get; set; }



    }
}
