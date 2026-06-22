# MilkbooksImageProcessor
Image Processor for the Unsplash API, built with C# and .NET 8.0.

Front end built in Angular, backend in C# .Net 8

.NET 8 chosen as it is the latest LTS.

I might have to update the image model - i am unsure how to represent the files. Probably two folders - 
One for 256px , one for 1024px, and one for originals.
Easiest to probably change the model to have the original, but then reference the 256/1024px version in the class.

We will populate the ImageProcessingModel first with the original variant, then add the small and thumb version

Scoped for the Download and resize services, so we have one per app run

In the front end, we can target the thumb versions of the variants to display in the UI

Prefer older, more conventional code, as it is easier to maintain by a wider amount of developers.
(Primary constructor c#12 sugar vs traditional constructor, for example)

Obfuscate the apikey in the env file so we dont leak it in the repo


APIKey in environment variable, even though it is just the client id, i don't own it, so i would
personally like to obfuscate it due to being uploaded to github. 