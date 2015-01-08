# IoT Smart home Hackathon Lab #

## Lab 2 Energy Monitoring ##

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

Following this, the Lab4-Visualisations provides

- **Azure Websites** to provide a website host, that features
	- **ASP.NET** to provide a web application framework 
	- **SignalR** to provide real time communication with browsers

### Key Challenge 1 - Device Security ###

A key challenge for the IoT is to secure communication from remote field devices with cloud services. The problem is made more difficult than other security scenarios on the web when we consider that although web protocols are used by remote services to provide a standard consumption model between devices, these devices do not all share common capabilities. For instance, many resource constrained devices are incapable of supporting SSL.

In order to solve this challenge, a cloud gateway can be operated which can receive communication from these resource constrained devices in a secure manner and then forward on to other services. The cloud gateway operates custom endpoints that can accept custom communications protocols designed for lightweight security. An example of these protocols is SSL-PSK which is a variant of the standard SSL protocol which uses a shared key embedded in devices, preventing the need for key exchange and reducing the overall resource requirements for securing communication.

This Key Challenge is implemented through the use of a cloud gateway provided by [Nitrogen.io](Nitrogen.md)

### Key Challenge 2 - Scalable Message Processing ###

A successful device on consumer sale can be measured in the millions of shipped units. Each device is capable of 'calling home', reporting their state to remote services, many times a second. Even with a relatively small number of distributed systems, it becomes a challenge to reliably receive and process the weight of messaging that Machine to Machine (M2M) communication makes possible. 

In order to solve this challenge, Cloud Platform members such as the Azure Event Hub can be used to offer message receipt and onward delivery. 

Furthermore, simply receiving messages is insufficient to meet the challenges of at scale message processing. We instead need to be able to pass these messages onto a processing 

This Key Challenge is implemented through the use of [Storm with HDInsight](README-STORM.md)

## Following on ##

The results of the message ingestion and processing can be visualised in Lab 4.
