using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reflection;
using YamlDotNet.RepresentationModel;

namespace Petrovich
{
    public partial class Petrovich
    {
        private static YamlNode RULES = ReadYamlFile();

        private static YamlNode ReadYamlFile()
        {
            /* чтение правил из ресурсов библиотеки
            Assembly _assembly = Assembly.GetExecutingAssembly();

            StreamReader input = new StreamReader(_assembly.GetManifestResourceStream("Petrovich.rules.yml"));
            */
            StreamReader input = new StreamReader("rules.yml");

            YamlStream yaml = new YamlStream();

            yaml.Load(input);

            return yaml.Documents[0].RootNode;
        }

        private class UnknownCaseException : Exception
        {
            public UnknownCaseException(string message)
                : base(message)
            {
            }
        }

        private class UnknownRuleException : Exception
        {
            public UnknownRuleException(string message)
                : base(message)
            {
            }
        }

        private class Rules
        {
            string _gender;

            public Rules(string gender = null)
            {
                this._gender = gender;
            }

            public string Lastname(string name, CASES gcase)
            {
                var lastname = (from r in (RULES as YamlMappingNode).Children
                                where (r.Key as YamlScalarNode).Value.Equals("lastname")
                                select r).FirstOrDefault().Value;

                return Inflect(name, gcase, lastname);
            }

            public string Firstname(string name, CASES gcase)
            {
                var firstname = (from r in (RULES as YamlMappingNode).Children
                                 where (r.Key as YamlScalarNode).Value.Equals("firstname")
                                 select r).FirstOrDefault().Value;

                return Inflect(name, gcase, firstname);
            }

            public string Middlename(string name, CASES gcase)
            {
                var middlename = (from r in (RULES as YamlMappingNode).Children
                                  where (r.Key as YamlScalarNode).Value.Equals("middlename")
                                  select r).FirstOrDefault().Value;

                return Inflect(name, gcase, middlename);
            }

            public bool Match(string name, YamlNode rule, bool matchWholeWord, YamlNode tags)
            {
                var tagRules = from r in (rule as YamlMappingNode).Children
                               where (r.Key as YamlScalarNode).Value.Equals("tags")
                               select r;

                if (!TagsAllow(tags, new YamlMappingNode(tagRules)))
                    return false;

                var gender = (from r in (rule as YamlMappingNode).Children
                              where (r.Key as YamlScalarNode).Value.Equals("gender")
                              select r).FirstOrDefault().Value;

                if (((gender as YamlScalarNode).Value.Equals("male") && this.Female) ||
                    ((gender as YamlScalarNode).Value.Equals("female") && !this.Female))
                    return false;

                var test = (from r in (rule as YamlMappingNode).Children
                            where (r.Key as YamlScalarNode).Value.Equals("test")
                            select r).FirstOrDefault().Value;

                name = name.ToLower();

                foreach (var s in (test as YamlSequenceNode))
                {
                    string value = (s as YamlScalarNode).Value;

                    var x = matchWholeWord ? name : name.Substring((int)Math.Max(name.Length - value.Length, 0));

                    if (x == value)
                        return true;
                }

                return false;
            }

            protected bool Male
            {
                get { return this._gender == "male"; }
            }

            protected bool Female
            {
                get { return this._gender == "female"; }
            }

            protected string Inflect(string name, CASES gcase, YamlNode rules)
            {
                int i = 0;

                string[] parts = name.Split('-');

                for (int y = 0; y < parts.Length; y++)
                {
                    bool firstWord = (i += 1) == 1 && parts.Length > 1;

                    parts[y] = FindAndApply(parts[y], gcase, rules, new bool[] { firstWord });
                }

                return string.Join("-", parts);
            }

            protected string Apply(string name, CASES gcase, YamlNode rule)
            {
                foreach (char c in ModificatorFor(gcase, rule))
                {
                    switch (c)
                    {
                        case '.':
                            break;
                        case '-':
                            name = name.Substring(0, name.Length - 1);
                            break;
                        default:
                            name += c;
                            break;
                    }
                }

                return name;
            }

            protected string FindAndApply(string name, CASES gcase, YamlNode rules, bool[] features)
            {
                try
                {
                    YamlNode rule = FindFor(name, rules, features);

                    return Apply(name, gcase, rule);
                }
                catch (UnknownRuleException ex)
                {
                    return name;
                }
            }

            protected YamlNode FindFor(string name, YamlNode rules, bool[] features)
            {
                YamlNode tags = ExtractTags(features);

                var exceptions = (from r in (rules as YamlMappingNode).Children
                                  where (r.Key as YamlScalarNode).Value.Equals("exceptions")
                                  select r).FirstOrDefault().Value;

                YamlNode p;

                if (exceptions != null)
                {
                    p = Find(name, exceptions, true, tags);

                    if (p != null && (p as YamlMappingNode).Children.Count > 0) return p;
                }

                var suffixes = (from r in (rules as YamlMappingNode).Children
                                where (r.Key as YamlScalarNode).Value.Equals("suffixes")
                                select r).FirstOrDefault().Value;

                if ((p = Find(name, suffixes, false, tags)) != null)
                    return p;
                else
                    throw new UnknownRuleException(string.Format("Cannot find rule for {0}", name));
            }

            protected YamlNode Find(string name, YamlNode rules, bool matchWholeWord, YamlNode tags)
            {
                foreach (var x in (rules as YamlSequenceNode).Children)
                    if (Match(name, x, matchWholeWord, tags))
                        return x as YamlMappingNode;

                return new YamlMappingNode();
            }

            protected string ModificatorFor(CASES gcase, YamlNode rule)
            {
                var mods = (from r in (rule as YamlMappingNode).Children
                         where (r.Key as YamlScalarNode).Value.Equals("mods")
                         select r).FirstOrDefault().Value;

                switch (gcase)
                {
                    case CASES.NOMINATIVE:
                        return ".";
                    case CASES.GENITIVE:
                        return ((mods as YamlSequenceNode).Children[0] as YamlScalarNode).Value;
                    case CASES.DATIVE:
                        return ((mods as YamlSequenceNode).Children[1] as YamlScalarNode).Value;
                    case CASES.ACCUSATIVE:
                        return ((mods as YamlSequenceNode).Children[2] as YamlScalarNode).Value;
                    case CASES.INSTRUMENTAL:
                        return ((mods as YamlSequenceNode).Children[3] as YamlScalarNode).Value;
                    case CASES.PREPOSITIONAL:
                        return ((mods as YamlSequenceNode).Children[4] as YamlScalarNode).Value;
                    default:
                        throw new UnknownCaseException(string.Format("Unknown grammatic case: {0}", gcase));
                }
            }

            protected YamlNode ExtractTags(bool[] features)
            {
                return new YamlMappingNode(features.Where(k => k).Select(k => new YamlScalarNode(k.ToString())));
            }

            protected bool TagsAllow(YamlNode tags, YamlNode ruleTags)
            {
                ruleTags = ruleTags ?? new YamlMappingNode();

                return (ruleTags as YamlMappingNode).Except(tags as YamlMappingNode).ToArray().Length == 0;
            }
        }
    }
}
