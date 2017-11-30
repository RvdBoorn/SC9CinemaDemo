#Sitecore 9 Cinema Demo

##Quick Install Guide

* Install Clean Sitecore 9 With XConnect (uses SC9Demo.sc as hostname, but this shouldn't matter)
* Install SC9DemoItems.zip using the package installer (this has all the site items, forms, media items, and other assets)
* Check the publish settings in Visual Studio (2015) so it publishes to your new sitecore instance
* Build the Solution
* Edit program.cs SitecoreCinemaModelGenerator project, so the File.WriteAllText on line 19 matches a local path
* Run the SitecoreCinemaModelGenerator project application, this should generate a file 'SitecoreCinemaModel, 1.0.json'
* Copy this file to: PathToXconnectSite\App_data\Models
* Copy this file to: PathToXconnectSite\App_data\jobs\continuous\IndexWorker\App_Data\Models
* Copy the SitecoreCinema.dll (From SitecoreCinema Project to) PathToXconnectSite\App_data\jobs\continuous\AutomationEngine
* Copy the SC9Demo.dll (From SC9Demo Project to) PathToXconnectSite\App_data\jobs\continuous\AutomationEngine
* Edit PathToXconnectSite\App_data\jobs\continuous\AutomationEngine|App_data|Config\Sitecore\Segmentation\sc.Xconnect.Segmentation.Predicates.xml

... add the following before the </PredicateDescriptors> tag
```xml
			<PurchasedTickets>
				<id>{D40601FA-496D-43B9-9B58-81EA65F9E344}</id>
				<type>SC9Demo.Rules.Conditions.PurchasedTickets, SC9Demo</type>
			</PurchasedTickets>
			<ReservedTickets>
				<id>{71D6314F-F54D-4469-9217-EDA98A2B9097}</id>
				<type>SC9Demo.Rules.Conditions.ReservedTickets, SC9Demo</type>
			</ReservedTickets>	