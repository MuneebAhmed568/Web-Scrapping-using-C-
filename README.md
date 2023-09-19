# Web-Scrapping-using-DotNet

# Automated Medicine Information Retrieval

This repository contains a console application designed to streamline the retrieval of detailed information for a large list of medicines. With thousands of medicines to process, the application automates this task efficiently.

## Features

### Part 1: Fetching Medicine Information
- The application accesses a database to retrieve a list of medicine names.
- It then initiates a search on the specified website 'https://sehat.com.pk/' for each medicine.
- The result includes valuable data such as the medicine's name and price.

### Part 2: Extracting Detailed Medicine Records
- Using the obtained medicine names, the application generates links to specific websites (please note that accuracy may vary).
- Once on these websites, it utilizes web scraping techniques to extract comprehensive information about each medicine.

## Usage
- Clone this repository to your local environment.
- Customize the database connection and website URLs as needed.
- Run the application to automate the medicine information retrieval process.

**Note:** For accurate data extraction, it is crucial to inspect the web page source and identify the precise HTML elements, including divs and CSS classes, required to fetch the correct information.

Feel free to contribute, report issues, or suggest improvements to enhance the functionality of this automated medicine information retrieval tool.
