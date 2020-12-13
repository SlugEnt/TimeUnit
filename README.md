# TimeUnit

The TimeUnit class is a C# immutable class (really a struct) that represents a time duration in a condensed form.  Examples are:
65S  = 65 milli-seconds
240s = 240 seconds
6m   = 6 Minutes
3h   = 3 hours
18d  = 18 days
7w   = 7 weeks

TimeUnits are valid between 0 mill-seconds and the max long weeks.  
The max supported time unit type is weeks.  The reason is that months are not a precise time period, some months have 30 days, some 31 days, some less.  So the conversion from a week or seconds to a month will never be precise, so at the moment we stop at weeks.  


## TimeUnit Types
TimeUnit supports the following unit types:
milliseconds
seconds
minutes
hours
days
weeks

TimeUnit only supports positive values.  

The class provides many methods of being able to convert the stored value into other values such as converting hours to minutes or seconds.


### Usage

```
#!CSharp
// The following are all valid ways of defining a TimeUnit that is 10 minutes.
TimeUnit t1 = new TimeUnit("10m");
TimeUnit t1 = new TimeUnit("600s");
TimeUnit t1 = new TimeUnit(600000);

TimeUnit t1 = 600000;
TimeUnit t1 = "10m";

```

The following are some of the ways to extract information out of the TimeUnit class.
```
#!CSharp
TimeUnit t1 = new TimeUnit("10m");

// string s1 will be 10m
string s1 = t1;     

// integer i1 will be 600000
int i1 = t1;  

// st2 will be 90m (90 minutes).
// ValueAsWholeNumber returns the largest unit that can be represent the TimeUnit as a whole number.
TimeUnit t2 = new TimeUnit (5400000);
string st2 = t2.ValueAsWholeNumber;

// Print as a string
st2.ToString;   // Produces 90 Minutes

// Print as its initial value:
TimeUnit t3 = "90m"
t3.Value;   // Produces 90m

// st3 will be 5399 milliseconds as it cannot be converted to a whole number any larger.
TimeUnit t4 = new TimeUnit (5399);
string st4 = t4.ValueAsWholeNumber;

// Return just the numeric part of the TimeUnit. 
// valA will be 19.
TimeUnit tA = "19d";
double valA = TA.ValueAsNumeric;



### TimeUnit Math!
The TimeUnit class supports math functions.

// Add 60minutes to DateTime object
TimeUnit t1 = "60m";
DateTime current = DateTime.Now;
DateTime future60m = t1.AddToDate(current);


// Add 2 TimeUnits together
TimeUnit a = new TimeUnit("59m");
TimeUnit b = "60s";
TimeUnit c = a + b;
C.ToString;     // 1 Hour


// Subtract 2 TimeUnits.  If the result is less than zero, then result is zero.
TimeUnit a = "12m";
TimeUnit b = "10m";
TimeUnit c = a - b;
C.ToString;   // 2 Minutes


// All Math comparison operators are supported
//   !=
//   == 
//   <
//   > 
//   <= 
//   >=

```

