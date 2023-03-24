Most of our projects are backed up by Azure background services for processing database records/documents/api data/edi files and this test assignment its simplified example of services we have.

Objective
  
  The objective of this test is for you to create a simple background service implemented as a console application with a scheduled, repeatable task that downloads weekly U.S. fuel pricing from “EIA open api” and saves it to the database. 


Technical Stack 
  
  Required

		Any .Net Framework supported by Microsoft

		SQL Server, or SQL Express to store the data

		For data access choose ADO.NET or any ORM library

	Optional

		Any third-party libraries you like for scheduling, downloading, and parsing results. 



Detailed Flow Description

	The background service should do the following:

	Download weekly fuel pricing from: https://api.eia.gov/v2/petroleum/pri/gnd/data/?frequency=weekly&data[0]=value&facets[series][]=EMD_EPD2D_PTE_NUS_DPG&sort[0][column]=period&sort[0][direction]=desc&offset=0&length=5000&api_key=EthXWE6eUTrBEJ1uTpNCqbL4NjghRxaC2R5tw1b2

	Parse the data and extract first series section in format [period, price]

	Do not save data to the database that is older than N days and save it to database in the format [period, price]

	If record already exists in database - ignore it, duplicates can be checked by the period field.



Requirements:

	Successfully create a background process/console application that follows the work flow is described in the previous section.

	Add two parameters that will allow us to configure the behavior of the service: 

		Task execution delay: Delay between background job executions.

		Days count:  This is the 'N days' stated above.

		Suggestion: These parameters could be passed to the background service by including them on a app.config parameter, or json settings, whatever you prefer.  