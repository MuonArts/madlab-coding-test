namespace codingtest;

// =======================================================================================================
// import libraries
using System;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// =======================================================================================================
// user defined structs

// struct for returning data from MakeGetRequest()
// holds the response, and a flag for whether the request was successful
struct RequestResponse
{
    public bool requestSuccess;
    public string responseString;

    // constructor. Required success and response values to be passed in on struct creation
    public RequestResponse(bool success, string response)
    {
        requestSuccess = success;
        responseString = response;
    }
}

// struct for returning data from CreateSearchResult()
// holds the parsed data from a successful search, and a flag for whether or not the struct has been successfully populated with parsed data
struct SearchResult
{
    public int count {get; set;}
    public string next {get; set;}
    public string previous {get; set;}
    public BookResult[] results {get; set;}
    public Boolean Populated = false;

    // constructor. No required values, all data except the populated flag will be filled by the json parser. Defaults to an unpopulated failed request result
    public SearchResult()
    {
        Populated = false;
    }
}

// =======================================================================================================
// API structs - implemented as described in https://gutendex.com/

struct Person
{
    public string name {get; set;}
    public int? birth_year {get; set;} // can have null values so must use nullable int
    public int? death_year {get; set;} // can have null values so must use nullable int
}

struct Format
{
    //unsure of what the documentation means by a MIME type, left unimplemented for now
}

struct BookResult
{
    public int id {get; set;}
    public string title {get; set;}
    public Person[] authors {get; set;}
    public Person[] translators {get; set;}
    public string[] subjects {get; set;}
    public string[] bookshelves {get; set;}
    public string[] languages {get; set;}
    public bool copyright {get; set;}
    public string media_type {get; set;}
    public Format formats {get; set;}
    public int download_count {get; set;}
}

// =======================================================================================================
// program entrypoint

class Program
{
    static async Task Main(string[] args)
    {
        // behaviour values
        // control how the input for the requested tasks is modified. Ideally would be somewhere suitable, eg asking the user, but document specifically requests these values
        string defaultPage = "http://gutendex.com/books/?page=1";
        int ageFilterThreshold = 200;
        string searchTitle = "Short Stories";
        string searchAuthor = "Dostoyevsky, Fyodor";

        // how many book results should be printed to the terminal before the remaining ones are truncated
        int searchResultCount = 5;

        // initialise references
        using HttpClient client = new HttpClient();
        SearchResult results = new SearchResult();

        Console.Clear();
        Console.WriteLine("MadLab Junior Developer Coding Test");

        // menu loop
        bool inMenu = true;
        while (inMenu)
        {
            // print menu to terminal. Options separated by newline (/n) and indented by 3 spaces
            Console.WriteLine("\nSelect an option to run a requested test:\n [1] Fetch from API\n [2] Arrays (Sort by ID)\n [3] Strings (Modify subjects to be uppercase)\n [4] Dates (Filtering Authors older than 200 years)\n [5] Find An Entry (Search from page 1 onwards until finding Short Stories by Dostoyevsky, Fyodor)\n [6] Exit");
            switch (Console.ReadLine()) // should separate out case handling into function calls for readability and maintainability
            {
                // cases 1 through 4 have boilerplate for fetching and validating the data. <--- maybe replace with macro? The function for case 5 will fetch and validate date itself
                case "1": // fetch page 1 from api
                {
                    // call http request wrapper function, wait until data is recieved
                    results = await CreateSearchResult(client, "http://gutendex.com/books/?page=1"); // explicitly page 1, not default page
                    if (results.Populated)
                    {
                        Task1(results, searchResultCount);
                    } else { Console.WriteLine("Recieved data was invalid, please try again"); } // output error
                    break;
                }
                case "2": // sort by ID, ascending with array.sort
                {
                    results = await CreateSearchResult(client, defaultPage); // call http request wrapper function, wait until data is recieved
                    if (results.Populated) // check if recieved result is valid
                    {
                        results = Task2(results); // pass recieved results into sorting function and save over old array
                        PrintSearchResult(results, searchResultCount); // print sorted array
                    } else { Console.WriteLine("Recieved data was invalid, please try again"); } // output error
                    break;
                }
                case "3": // using array.map modify each item's subjects to be uppercase
                {
                    results = await CreateSearchResult(client, defaultPage); // call http request wrapper function, wait until data is recieved
                    if (results.Populated) // check if recieved result is valid
                    {
                        results = Task3(results); // pass recieved results into modifying function and save over old array
                        PrintSearchResult(results, searchResultCount); // print sorted array
                    } else { Console.WriteLine("Recieved data was invalid, please try again"); } // output error
                    break;
                }
                case "4": // using array.filter remove all entries whose author didn't exist within the last 200 years
                {
                    results = await CreateSearchResult(client, defaultPage); // call http request wrapper function, wait until data is recieved
                    if (results.Populated) // check if recieved result is valid
                    {
                        results = Task4(results, ageFilterThreshold); // pass recieved results into modifying function and save over old array
                        PrintSearchResult(results, searchResultCount); // print sorted array
                    } else { Console.WriteLine("Recieved data was invalid, please try again"); } // output error
                    break;
                }
                case "5": // iterate through the data until finding "Short Stories" by "Dostoyevsky, Theodor". If it is not on the page, go to the next
                {         // result is actually in page 11, not 12 as the document states
                    // call async search task. Multiple searches may be needed, so cannot be done and validated before executing function for the task
                    BookResult searchOutput = await Task5(client, searchTitle, searchAuthor);
                    if (searchOutput.title == searchTitle) // if search succeeded, result title should match searched for title
                    {
                        Console.WriteLine("Search Successful!");
                        PrintBookResult(searchOutput);
                    } else // failed search will return an empty BookResult struct, so title will not match
                    {
                        Console.WriteLine("Search was unable to find " + searchTitle + " by " + searchAuthor);
                    }
                    break;
                }
                case "6": // exit program
                {
                    inMenu = false; // set flag for loop to false
                    break;
                }
                //
                //   (add extra cases for more actions here)
                //
                //
                default: // if no case is met, ie an invalid input was given, display error and ask for input again
                {
                    Console.WriteLine("Please choose a valid option");
                    break;
                }
            }   
        }
        //Console.WriteLine("EXIT");
    }

// =======================================================================================================
// define task functions

    static void Task1(SearchResult results, int displayedResultsCount) // fetch page 1 from api
    {
        // display the recieved results
        PrintSearchResult(results, displayedResultsCount);
    }

    static SearchResult Task2(SearchResult searchResultToSort) // sort by ID, ascending with array.sort
    {
        // user defined struct has no default sorting method, so sort by property with lambda expression
        Array.Sort<BookResult>(searchResultToSort.results, (x,y) => x.id.CompareTo(y.id));
        // return sorted array
        return searchResultToSort;
    }

    static SearchResult Task3(SearchResult searchResultToModify) // using array.map modify each item's subjects to be uppercase
    {
         // modify array (array.map is not in c#, linq Select() is the equivalent)
         // due to layout of data structs, nested Select() needed. Iterating with a nested loop might be preferred
         // outer Select()
        searchResultToModify.results = searchResultToModify.results.Select(a =>
        {
            // inner Select(), set each value in subject array to be upper case
            a.subjects = a.subjects.Select(b => b.ToUpper()).ToArray();
            return a;
        } ).ToArray();
        // return modified array
        return searchResultToModify;    
    }

    static SearchResult Task4(SearchResult searchResultToFilter, int ageToFilter) // using array.filter remove all entries whose author didn't exist within the last 200 years
    {
        // modify array (array.filter is not in c#, Array.FindAll is the equivalent)
        // instead of array.filter(function), Array.FindAll and a lambda expression is used to do the comparison and discard items that do not match
        searchResultToFilter.results = Array.FindAll(searchResultToFilter.results, bookResult => 
        {
            foreach (Person author in bookResult.authors) // loop through authors array in case multiple are listed, only one must pass the check
            {
                // initialise local variables
                bool authorDateValid = false;
                int authorDate = 0;
                // find value to use in comparison. Check birth and death years separately in case one field is null
                if (author.birth_year != null) // check if a valid birth year is in the data
                {
                    authorDate = (int)author.birth_year;
                    authorDateValid = true;
                }
                if (author.death_year != null) // check if a valid death year is in the data, done second as will always be greater than birth year
                {
                    authorDate = (int)author.death_year;
                    authorDateValid = true;
                }
                // run comparison. If no valid birth or death date was found, skip and assume false
                if (authorDateValid && (DateTime.Now.Year - authorDate) < ageToFilter)
                {
                    return true; // if this author passes the check, return early, no need to check the remaining authors in the array
                }
            }
            return false; // if no authors pass the check, return false to discard this book from the list
        });
        // return filtered array
        return searchResultToFilter;
    }

    static async Task<BookResult> Task5(HttpClient client, string searchedTitle, string searchedAuthor) // iterate through the data until finding "Short Stories" by "Dostoyevsky, Theodor". If it is not on the page, go to the next
    {
        // call http request wrapper function, wait until data is recieved
        SearchResult results = await CreateSearchResult(client, "http://gutendex.com/books/?page=1"); // explicitly page 1, not default page
        while (true) // search loop
        {
            if (results.Populated == false) // check if request results are valid
            {
                Console.WriteLine("Search Failed: Request Returned Invalid Data");
                return new BookResult(); // return empty book result 
            }
            foreach (BookResult book in results.results) // iterate through request results to check is an entry matches the searched title and author
            {
                // check if this book matches the title and author being searched for
                // title is an easy comparison, being an accessible field in the outermost SearchResult struct
                // author name check requires array manipulation, convert the Person array in the book's authors field into an array of the author names, then check if the new array contains the searched for author
                if (book.title == searchedTitle && book.authors.Select(person => person.name).ToArray().Contains(searchedAuthor))
                {
                    return book;
                }
            }
            if (results.next == null) // check for last page
            {
                return new BookResult(); // return empty book result            
            }
            else // if the searched for book has not been found, and a valid next page exists, request the next page data, and continue
            {
                Console.WriteLine("Not found on current page, searching " + results.next);
                results = await CreateSearchResult(client, results.next); // call http request wrapper function. Do not need to check validity, this is done in the next loop iteration
            }
        }                   
    }

// =======================================================================================================
// define program functions

// function that handles the http request, will return the response as a string, and flag for whether or not the request was successful
    static async Task<RequestResponse> MakeGetRequest(HttpClient client, string url)
    {
        // check if client is valid, if not, return with error
        if (client == null)
        {
            //Console.WriteLine("Error: Http client has not been initialised");
            return new RequestResponse(false, "Error: Http client has not been initialised");
        }
        
        // initialise response fields
        HttpResponseMessage response;
        string responseBody= "";

        // attempt to make the get request
        try
        {
            response = await client.GetAsync(url);
        }
        catch (HttpRequestException exception)
        {
            //Console.WriteLine("Request Failed: " + exception.HttpRequestError + ", " + exception.Message);
            return new RequestResponse(false, "Request Failed: " + exception.HttpRequestError + ", " + exception.Message);
        }
        catch (TaskCanceledException)
        {
            //Console.WriteLine("Request Failed: Timed Out");
            return new RequestResponse(false, "Request Failed: Timed Out");
        }

        switch (response.StatusCode) // switch on status code recieved in response, unhandled cases will fall through to default and return an error.
        {
            case System.Net.HttpStatusCode.OK: // expected response
            {
                responseBody = await response.Content.ReadAsStringAsync();
                return new RequestResponse(true, responseBody);
            }
            //
            //   (add extra cases for other response codes here if needed)
            //
            //
            default: // default case for all other responses not explicitly handled
            {
                //Console.WriteLine("Error: Unhandled Response: " + (int)response.StatusCode + " " + response.StatusCode);
                return new RequestResponse(false, "Error: Unhandled Response: " + (int)response.StatusCode + " " + response.StatusCode);
            }
        }
    }

// wrapper function for MakeGetRequest() to make usage simpler. Handles error checking and data parsing, returning recieved data in a parsed SearchResult structure
// returned structure has a flag for whether the data has been successfully parsed and populated into the struct
    static async Task<SearchResult> CreateSearchResult(HttpClient client, string url)
    {
        Console.WriteLine("Making http Request...");
        // make http request using MakeGetRequest() defined above
        RequestResponse response = await MakeGetRequest(client, url);
        if (response.requestSuccess == true)
        {
        // request successful, parse recieved data string
            Console.WriteLine("Request Successful");
            SearchResult result = JsonSerializer.Deserialize<SearchResult>(response.responseString);
            result.Populated = true; // set populated flag to true so later checks will know the data is valid
            return result;
        } else
        {
            // request failed. Error response string placed in responseString field
            Console.WriteLine(response.responseString);
            return new SearchResult(); // return empty SearchResult struct, populated flag is false by default
        }
    }

// helper function to convert a BookResult struct into a formatted output for the terminal
// separated out from PrintSearchResult for use on its own elsewhere
    static void PrintBookResult(BookResult book)
    {
        // could be combined into a single WriteLinecall, but easier to read/edit separately
        Console.WriteLine(book.id + " - " + book.title);
        Console.WriteLine("   Subjects: " + CreateArrayString(book.subjects));
        Console.WriteLine("   Authors: " + CreatePersonString(book.authors));
        Console.WriteLine("   Translators: " + CreatePersonString(book.translators));
        Console.WriteLine("   Bookshelves: " + CreateArrayString(book.bookshelves));
        Console.WriteLine("   Languages: " + CreateArrayString(book.languages));
        Console.WriteLine("   " + book.media_type + ", " + book.download_count + " downloads");
    }

// helper function to display stored search result data in a user friendly way in the terminal
// a requested number of entries will be shown, with the rest being truncated with a message stating how many were not shown
    static void PrintSearchResult(SearchResult result, int entriesToPrint)
    {
        // ensure for loop never indexes array out of bounds
        int iterationBound = Math.Min(entriesToPrint, result.results.Length);
        for (int i = 0; i < iterationBound; i++)
        {
            // pass to PrintBookResult()
            PrintBookResult(result.results[i]);
        }
        // check if truncation message should be shown
        if (iterationBound < result.results.Length)
        {
            Console.WriteLine("...(" + (result.results.Length - iterationBound) + " remaining entries truncated)" );
        }
    }

// overload method for PrintSearchResult() so that number of entries to print is optional, and will default to 5 if not stated
    static void PrintSearchResult(SearchResult result)
    {
        PrintSearchResult(result, 5);
    }

// helper function for converting an array of person structs into a user friendly string for displaying in the terminal
    static string CreatePersonString(Person[] people)
    {
        string tempString = ""; // initialise empty string to append formatted person entries
        for (int i = 0; i < people.Length; i++)
        {
            // check if a separator should be added between entries
            if (i > 0)
            {
                tempString += ", ";
            }
            
            tempString += people[i].name + " (";

            if (people[i].birth_year == null)
            {
                tempString += "unknown-";
            } else if (people[i].birth_year < 0) // check for negative numbers to display as BC to clean up formatting
            {
                tempString += (Math.Abs((int)people[i].birth_year)) + "BC-"; // null already checked for before casting
            } else
            {
                tempString += people[i].birth_year + "-";
            }

            if (people[i].death_year == null)
            {
                tempString += "unknown)";
            } else if (people[i].death_year < 0) // check for negative numbers to display as BC to clean up formatting
            {
                tempString += (Math.Abs((int)people[i].death_year)) + "BC)"; // null already checked for before casting
            } else
            {
                tempString += people[i].death_year + ")";
            }
        }
        if (tempString == "") // if people array was empty, default to "None" for cleaner formatting
        {
            tempString = "None";
        }
        return tempString; // return built string
    }

// helper function for converting an array of strings to a user friendly string for displaying in the terminal
    static string CreateArrayString(string[] stringArray)
    {
        string tempString = ""; // initialise empty string to append formatted strings
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (i > 0) // check for adding separaters
            {
                tempString += ", ";
            }
            tempString += stringArray[i]; // append the currently indexed string
        }
        if (tempString == "") // if people array was empty, default to "None" for cleaner formatting
        {
            tempString = "None";
        }
        return tempString; // return built string
    }
}
