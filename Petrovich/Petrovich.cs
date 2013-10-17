using System;

namespace Petrovich
{
    public enum CASES
    {
        NOMINATIVE,
        GENITIVE,
        DATIVE,
        ACCUSATIVE,
        INSTRUMENTAL,
        PREPOSITIONAL
    }

    public partial class Petrovich
    {
        public Petrovich(string gender = null)
        {
            this._gender = gender;
        }

        public string Lastname(string name, CASES gcase)
        {
            return new Rules(this._gender).Lastname(name, gcase);
        }

        public string Firstname(string name, CASES gcase)
        {
            return new Rules(this._gender).Firstname(name, gcase);
        }

        public string Middlename(string name, CASES gcase)
        {
            return new Rules(this._gender).Middlename(name, gcase);
        }

        private string _gender;

        public string Gender
        {
            get { return this._gender; }
        }

        public string DetectGender(string midname)
        {
            switch ((midname.Substring(midname.Length - 2)).ToLower())
            {
                case "ич":
                    return "male";
                case "":
                    return "female";
                default:
                    return "androgynous";
            }
        }

        
    }
}