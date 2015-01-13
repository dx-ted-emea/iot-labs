#Batch Processing with Hive/Sqoop#


The following details how to **manually** run a Hive job to process the energy readings that have been written to blob storage.  This agregates the data and calculates averages per hour per device.  The result will be written to a MSSQL database using Sqoop.

While this is currently a manual process, it can be automated using Oozie

###Pre-requisites###

- Setup hadoop cluster in Azure.  Ensure the Storage Account is the same as the one prepared for the **worker role** configured below (**The one that pulls from the Event Hub to blob storage**)
- Enable remote access to the cluster
- Download JSON-Serde, for best results you should get it from this repository, the binary jar file is located in the `dist` directory:

	https://github.com/sheetaldolas/Hive-JSON-Serde/tree/master

- Create new directory `jars` in the storage account created when setting up the cluster
- Upload the jar to that location
- Create a MS SQL instance in Azure, copy the JDBC connection string **(for simplicity avoid special characters in the password, ! character for instance causes problems when running the Sqoop job**)

- Start the `IotEnergyBlob` worker role (it can run in the local Azure Compute Emulator); read [Worker Role](WorkerRole.md) for full details on how to set it up.

###Create database table###

- Connect to the MSSQL database and execute the following script to create a new database.

```sql
CREATE TABLE averagesPerHour (
[deviceId]  VARCHAR (255) NOT NULL,
[hourOfDay] VARCHAR (2)   NOT NULL,
[average]   INT           NOT NULL,
PRIMARY KEY (deviceId, hourOfDay))
```

###Running the Hive Job###

Apache Hive provides a means of running MapReduce job through an SQL-like scripting language, called HiveQL. Hive is a data warehouse system for Hadoop, which enables data summarization, querying, and analysis of large volumes of data.

- Navigate to https://<yourclustername>.azurehdinsight.net
- Navigate to the Hive Editor tab
- Execute the following script
 - You will need to change the location of the `energyreadings` external table to point to where the log files were written, e.g. `/2015/1/13`
 - You will also need to adjust the wasb:// URI to point to the right container, `batchfiles` in this example

```sql
ADD JAR wasb:///jars/json-serde-1.1.9.9-Hive13-jar-with-dependencies.jar;
set hive.exec.dynamic.partition = true;
set hive.exec.dynamic.partition.mode = nonstrict;

CREATE EXTERNAL TABLE energyreadings (
timestamp string, deviceId string, startReading int, endReading int, energyUsage int
)
--PARTITIONED BY (year string, month string, day string)
ROW FORMAT 
serde 'org.openx.data.jsonserde.JsonSerDe'
STORED AS TEXTFILE
LOCATION 'wasb://batchfiles@tomiot.blob.core.windows.net/2015/1/13';

create external table averagesByHour (device string, hour string, average string) 
row format delimited 
fields terminated by '\t' 
lines terminated by '\n' 
stored as textfile location 'wasb:///output';
  
insert into table  averagesByHour select deviceId, hour(from_unixtime(unix_timestamp(timestamp, "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'"))), avg(energyUsage) from energyreadings where deviceId is not NULL group by hour(from_unixtime(unix_timestamp(timestamp, "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'"))), deviceId ;
```

- Monitor the job and once complete navigate the storage account and examine the file in the output folder.  This would be expected to contain values which we can write to a database.

- If you need to run the script several times, here is how to drop the external tables and start again:

```sql
ADD JAR wasb:///jars/json-serde-1.1.9.9-Hive13-jar-with-dependencies.jar;
drop table energyreadings;
drop table averagesByHour;
```

###Sqoop###

Sqoop is a tool designed to transfer data between Hadoop clusters and relational databases. You can use it to import data from a relational database management system (RDBMS) such as SQL or MySQL or Oracle into the Hadoop Distributed File System (HDFS), transform the data in Hadoop with MapReduce or Hive, and then export the data back into an RDBMS. 

- Connect to the HDInsight cluster via RDP and start the Hadoop command line
- Navigate to C:\apps\dist\sqoop-1.4.4.2.1.9.0-2196\bin
- Execute the following command (modify as appropriate)

	`sqoop export --connect "jdbc:sqlserver://<server>.database.windows.net:1433;database=<database>;user=<userame>@<server>;password=<password>;encrypt=true;Trusted_Connection=True;trustServerCertificate=true;Encrypt=True;loginTimeout=30;" --table averagesPerHour --export-dir /output --input-fields-terminated-by \0x09 -m 1`

- Once complete data should be available in the database
