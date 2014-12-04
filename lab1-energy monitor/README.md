## IoT Smart home Hackathon Lab ##

### Lab 1 Energy Monitoring ###

Monitoring Energy is a key objective of the Smart home. In this lab we will take messages from a Smart Home, process them using Cloud resources and provide an interface to visualise these in a web browser.

Technology that we will use includes:

- **Arduino** Due to simulate an Energy Monitor
- **Nitrogen.io** to provide a Cloud Gateway
- **Azure Event Hub** to provide scalable message ingress
- **Azure HDInsight** to provide a managed Big Data analytics platform including
	- **Apache Hadoop** for batch processing
	- **Apache Storm** for stream processing
- **Azure SQL DB** to provide a relational data store
- **Azure Redis Cache** to provide a managed transient data store
- **Azure Websites** to provide a website host, that features
	- **ASP.NET** to provide a web application framework 
	- **SignalR** to provide real time communication with browsers

### Key Challenge 1 - Device Security ###

A key challenge for the IoT is to secure communication from remote field devices with cloud services. The problem is made more difficult than other security scenarios on the web when we consider that although web protocols are used by remote services to provide a standard consumption model between devices, these devices do not all share common capabilities. For instance, many resource constrained devices are incapable of supporting SSL 