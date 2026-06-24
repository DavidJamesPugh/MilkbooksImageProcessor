# MilkbooksImageProcessor

Input a query string to search images from the Unsplash API. Images are displayed in the UI.

Hovering on that image will show options to download the Full, Thumb, or Small size of that image. 
Downloading the Zip will download all images currently displayed

Clicking the previous search term will search that term again

Images older than 1 hour are deleted from the server  every 15 minutes to save storage space.

A toast popup will show any errors or success messages.

DockerFile is included to deploy on Render or other containerised environments


To run: 
cd Milkbooks.Client
ng build --configuration production

This will copy the built files to the Milkbooks.Server/wwwroot folder. 
Build the C# server project. 
Then run the server project to start the application.

Navigate to http://localhost:8080/app to view the application.

# Next steps for any deployment:
-Obfuscate ClientAPIKey if deploying to a wider userbase
-A history page with previous search terms that snapshot those images
-A favourites page to save images to a local database and view them later
