## Device Reliability

### Rationale
It is imperative that we do not react to deviation from normal operating parameters; otherwise a jump from 18 &deg;C to 180 &deg;C will result in heaters being disabled - whereas in reality we have more likely a problem with a sensor. 

In order to achieve this, we can use Machine Learning, and the AzureML product in particular, to calculate the likelihood of a reading being accurate.

### Implementation

For this lab, **FailureConditions** represents an important part of the design. We'll be using this value to inform a machine learning algorithm whether the device is failing by looking at a device which is generating a slight skew over time. While our particular usage of Machine Learning will not look at the temperature but will treat the device anomaly as an outlier this could be extended to look at this aspect.

In order proceed we'll need to setup and AzureML namespace. To do this you need to be logged into the Azure portal and have also enabled AzureML since it's still in preview. In order to work out how to do this follow the instructions in the following link.

[Enable AzureML as a preview feature](http://azure.microsoft.com/en-gb/services/preview/)

To begin create a new namespace using the **New** button at the bottom of the portal screen.

![Create an AzureML namespace](images/create%20azure%20ml%20namespace.png)

You should notice that the owner of the "workspace" is the logged in user. You'll also notice that as this is still in preview it is only available in **South Central US** so you'll need a storage account in that location. The Creation process will give you the option of creating one if one doesn't exist. After creating it you should see it running under the **machine learning** tab.

![Running machine learning workspace](images/running%20machine%20learning.png)

We'll be creating a new **experiment** now which will allow us to create a new ML service. To begin we'll click on the **Open in ML Studio** button at the bottom of the portal. 

![Create a new experiment](images/ml%20studio%20create%20new%20experiment.png)

Now we can upload a dataset containing devices and their respective temperatures for each minute time period. We won't be analysing this in a time-sensitive manner so the only relevant data point in this case will be temperature. The dataset is hotpath.tsv and can be found in the current directory.

![Upload a dataset](images/add%20a%20dataset.png)

We'll now see this on the **Saved Datasets** section of the workspace and can drag this across onto a workspace. It has been named IoT Temperature Data but look for the name you gave to the file.

You can now consume the dataset and project columns out 

![Experiment with projected columns](images/experiment%20with%20projected%20columns.png)

To get to this we will need to drag **Data Transformation/Project Columns** from the left hand menu and then select properties and include only temperature.

![Select temperature column](images/include%20columns%20in%20projection.png)

We'll be looking at at a technique called K-Means Clustering which will spot the anomalies and assign them to clusters. It will automatically try and pick out the number of clusters from the trained dataset we are presenting it with. In a normal machine experiment we would normally use only a %'age of data to **train** our K-Means clustering model but in this case we'll use 100% of our dataset and see if it picks out the skewed device data in a separate cluster. 

![Completed clustering model](images/complete%20model.png)

In order to complete the model drag **Machine Learning/Initialize Model/Clustering/K-Means Clustering** from the left hand menu then connect this to **Train Clustering Model** which can be dragged from **Machine Learning/Train/Train Clustering Model** and finally **Machine Learning/Score/Assign To Clusters** follow the other connections in the image from **Project Columns**. It's important to customise the **Train Clustering Model** and **Assign To Clusters** by selecting select columns and choosing temperature as you did in the previous step **Project Columns**

We can now select **Run** from the bottom menu.

We can now right-click on the output of the Assign To Clusters step and choose **Visualize** from the context menu. You should see a visualisation similar to that of below which shows that all of the normal values have been correctly assessed to be in **Cluster 0** and the two skewed values in **Cluster 1**

![Visualizing clusters](images/visualise%20clusters.png)

We have no further use of our trained model so we can save this to ensure that we don't need to calculate this every time we want to predict whether a device data point is in Cluster 0 or Cluster 1. To do this we can right-click on the left output node of the Train Clustering Model and select **save trained model** from the context menu.

![Save trained model](images/save%20trained%20model.png)

Enter the correct information in the dialog.

![Save trained model dialog](images/save%20trained%20model%dialog.png)

We can now remove the K-Means Clustering and Train Clustering Model step and use our trained model. This should be available to us from the left hand menu as per the diagram. We can simply wire this up to the **Assign To Clusters** along with Project Columns as per the image below.

![Final model with trained dataset](images/final%20model%20with%20trained%20dataset.png)

The final step now is to expose this model so that it can be consumed from a service which will be able to pass the data points to it every minute. 

Now we can set both the published inputs and outputs by clicking on the right hand input of **Assign To Clusters** and selecting "Set as Published Input". Similarly we can right click on the output and select "Set as Published Output". Now we just need to press **Run** on the bottom menu and when this completes we should have a new button available to us **PUBLISH WEB SERVICE**

This should only take a few seconds to complete and will take us to a new page which will show us how we can consume the web service to send single temperature points to determine the clusters.

You should be able to see a web service in the ML Studio portal now.

![The new web service](images/web%20service.png)

Clicking on the web service will reveal details of how to invoke them using either C#, Python or R. We should be able to lift the entire code section for C# and add this to a worker role which will process a set of data points returned from a SQL query that has been processed by Stream Analytics. Each row temperature can then be sent to the web service after being requested on every minute on a timer.

To check whether the web service is processing and giving us the correct outputs we can click on the **test** link which will give us a dialog enabling us to enter a temperature value.

![Test a temperature prediction cluster](images/predict%20and%20test%20with%20ml.png)
