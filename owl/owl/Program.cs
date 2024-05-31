using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace owl
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("  ============ The Owl asks 'Who?' ============ ");


            //string strNameToSearch = "t*y";
            //Console.WriteLine("  Searching fFor: " + strNameToSearch);


            var forest = Forest.GetCurrentForest();

            foreach (ActiveDirectorySite site in forest.Sites)
            {

                //Locate the SITE
                Console.WriteLine("Site: " + site.Name);
                foreach (DirectoryServer server in site.Servers)
                {

                    //Locate the SERVER
                    Console.WriteLine("\n\n\n ================================= Server: " + server.Name + " =================================\n\n");


                    DirectoryEntry rootEntry = new DirectoryEntry("LDAP://" + server.Name);
                    rootEntry.AuthenticationType = AuthenticationTypes.None; //Or whatever it need be
                    DirectorySearcher searcher = new DirectorySearcher(rootEntry);



                    //Seek specific types of people.
                    //LDAP query structure is wild.

                    //this will search a name.
                    //var queryFormat = "(&(objectClass=user)(objectCategory=person)(mail=*)(|(SAMAccountName=*{0}*)(cn=*{0}*)(gn=*{0}*)(sn=*{0}*)(email=*{0}*)))";
                    //searcher.Filter = string.Format(queryFormat, strNameToSearch);


                    //this can be tweaked to search a _type_ of user.
                    searcher.Filter = "(&(objectClass=user)(objectCategory=person)(mail=*))";





                    foreach (SearchResult result in searcher.FindAll())
                    {

                        //  Some specific fField names:
                        // https://docs.secureauth.com/0903/en/active-directory-attributes-list.html



                        //We can choose to turn the Groups off.  They get rather long.
                        bool boolShowGroupNames = true;
                        string strGroups;
                        if (boolShowGroupNames)
                        {
                            List<string> lstGroups = new List<string>();
                            foreach (string group in result.Properties["memberOf"])
                            {
                                lstGroups.Add(group);
                            }
                            lstGroups.Sort();
                            strGroups = "\n    " + String.Join("\n    ", lstGroups) + "\n";
                        }
                        else
                        {
                            strGroups = result.Properties["memberOf"].Count.ToString() + " Groups";
                        }



                        //People have a bunch of email addresses sometimes.
                        string strMails = "";
                        foreach (string mail in result.Properties["mail"])
                        {
                            //strMails += mail + "   ";
                        }
                        foreach (string mail in result.Properties["proxyAddresses"])
                        {
                            strMails += mail + "   ";
                        }



                        //Locked Out Status, and other possible fFlags
                        int intAccountStatus = int.Parse(result.Properties["userAccountControl"][0].ToString());
                        bool boolLockedOut = Convert.ToBoolean(intAccountStatus & 2);




                        if (true)
                        {
                            Console.WriteLine("cn: {0,-25}  department: {1}\n  mail: {2}\n  memberOf: {3}\n",
                            result.Properties["cn"].Count > 0 ? result.Properties["cn"][0] : string.Empty,
                            result.Properties["department"].Count > 0 ? result.Properties["department"][0] : string.Empty,
                            strMails,
                            strGroups
                            );
                        }





                        if (false)
                        {
                            //4096 is like, rooms i think
                            if (intAccountStatus < 4096)
                                if (!boolLockedOut)
                                {

                                    Console.WriteLine("{1,-25} - {0}      userAccountControl: {2} (Locked out:{3})\n",
                                        result.Properties["cn"].Count > 0 ? result.Properties["cn"][0] : string.Empty,
                                        result.Properties["department"].Count > 0 ? result.Properties["department"][0] : string.Empty,
                                        result.Properties["userAccountControl"].Count > 0 ? result.Properties["userAccountControl"][0] : string.Empty,
                                        boolLockedOut
                                        );

                                }
                        }




                        /*
                        //This section is EXHAUSTIVE.  But i dunno maybe you want that.
                        foreach (DictionaryEntry prop in result.Properties)
                        {
                            DictionaryEntry x = prop;
                            //Console.WriteLine(x);

                            Console.WriteLine(
                                "{0} : {1}",
                                x.Key.ToString(),
                                    result.Properties[x.Key.ToString()].Count == 1
                                    ?
                                    result.Properties[x.Key.ToString()][0]
                                    :
                                    "( " + result.Properties[x.Key.ToString()].Count + " Values )"
                            );
                        }
                        */





                        //end of user
                        Console.WriteLine();
                        //Console.WriteLine("\n\n");



                    }

                    //end of server
                    Console.WriteLine("\n\n");

                }
                //end of fForest
                Console.WriteLine("\n\n");
            }

            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }
    }
}
