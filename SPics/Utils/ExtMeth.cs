using SPics.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SPics.Utils
{
    internal static class ExtMeth
    {
        public static IEnumerable<string> ReplaceInCollection(this IEnumerable<string> source, string originalText, string newText)
        {
            List<string> coleccion = new List<string>();

            foreach (var item in source)
            {
                if (item.Contains(originalText))
                {
                    string aux = item;
                    aux = aux.Replace(originalText, newText);
                    coleccion.Add(aux);
                }
                else
                {
                    coleccion.Add(item);
                }
            }

            return coleccion;
        }

        public static T IsNotNull<T>(this T source)
        {
            if (source != null)
                return source;

            throw new ArgumentNullException();
        }

        public static string ExistsThisFile(this string source)
        {
            if (File.Exists(source))
                return source;

            throw new FileNotFoundException();
        }

        public static string CombineThisRelativeWithCompletePath(this string source)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), source));
        }

        public static object LoadThisFileWithRelativePath(this string source)
        {
            var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), source.IsNotNull()));

            return File.ReadAllText(path.ExistsThisFile());
        } 

        public static string[] SplitByString(this string origin, string SplitString)
        {
            return origin.Split(new string[] { SplitString }, StringSplitOptions.None);
            //return Regex.Split(origin, SplitString);
        }
      
        public static bool IsAnyItemNull<T>(this List<T> myList)
        {
            return myList.Any(x => x == null);
        }

        public static List<int> GiveMeNoConsecutives(this List<int> list)
        {
            var tempList = list.ToList();

            //var isConsecutive = !tempList.Select((i, j) => i - j).Distinct().Skip(1).Any();

            var result = tempList.Aggregate(
                        new List<Tuple<int, int>>(),
                        (l, i) =>
                        {
                            var last = l.LastOrDefault();
                            if (last == null ||
                            last.Item1 + last.Item2 != i)
                            {
                                l.Add(new Tuple<int, int>(i, 1));
                            }
                            else if (last.Item1 + last.Item2 == i)
                            {
                                l.RemoveAt(l.Count - 1);
                                l.Add(new Tuple<int, int>(last.Item1, last.Item2 + 1));
                            }

                            return l;
                        },
                                l => l)
                                .Select(x => /*new List<int>*/  x.Item1 + x.Item2).ToList();

            return result;
        }


        public static bool Run<T>(this T source)
        {
            source.IsNotNull();

            Type t = source.GetType();

            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);

                if (value != null && (string)value != "")
                {
                    return false;
                }
            }

            return true;
        }

        public static Pic NewPic(string name, string path,
            System.Drawing.Image img = null, List<string> tags = null, string tagsAsString = null, string tagsForUI = null)
        {
            return new Pic
            {
                Name = name,
                Path = path,
                Image = img,
                Tags = tags,
                TagsAsString = tagsAsString,
                TagsForUI = tagsForUI
            };
        }


    }
}
