<strong>(This repo is work in progress - most code should be there, but I'm working on better documenting it and improving the code which evolved during many late night research hacking sessions! This is the first time I've extensively released research code - so, if you have any feedback then please let me know.)</strong>


<h2>Introduction</h2>

This git repo contains the accompanying query auto-completion (QAC) approach implementations detailed in the paper "Recent and Robust Query Auto-completion" (Stewart Whiting &amp; Joemon M. Jose - University of Glasgow), presented at the World Wide Web Conference (WWW) 2014 in Seoul/South Korea. 

The central idea of this paper is simply: can QAC systems be optimised to support both always popular (i.e. robustness), and recently popular queries? You can read the full paper at: http://www.stewh.com/wp-content/uploads/2014/02/fp539-whiting.pdf. It is written to be accessible to as many readers as possible. Further elaboration on some of the techniques will be included in my thesis - whenever it is finished!

This source code will be helpful to researchers looking to re-implement and improve on these baseline datasets and approaches. Furthermore, practitioners might find some inspiration for implementing their own QAC systems - though bear in mind the objective of this code is for research, so it is far from optimal for production use! 

Although we use the AOL/MSN/Sogou datasets in the paper, any dataset in a similar input format may be used.

For those wanting to reproduce the experiments in the paper (i.e. to use as a baseline), you will need to prepare the 'typed query' datasets from the raw AOL/MSN/Sogou query logs, using the code contained in the 'Extract*' projects.

<h2>Source Code</h2>

The code is all C# .Net 4.5 and built in Visual Studio 2013. Heavy use is made of threading and the Task Parallel Library and Amib SmartThreadPool for multi-threaded optimisation where possible. I have set the project explicitly to build in 64bit since the memory requirements will be quite high for some QAC approaches.

<strong>Note:</strong> In the code, we refer to the 'LNQ' (i.e. Last N queries) approach as 'NTB' (i.e. 'non temporal bucket'). This was an early research convention that has remained in the code. Apologies for the inconsistency - I will change it when I get a chance, but there are a lot of references to NTB!

<h2>System Requirements</h2>
I run 2 instances of this code simultaneously on a 4 core i7 with 24gb of RAM. The high performing LNQ approaches run relatively efficiently, so you shouldn't need as much memory for them. Naturally, running time varies per approach and dataset.

<h2>Creating the Typed Query Datasets for Experiments</h2>

The size of each typed query log files is as follows:
AOL - 705,244,335 bytes (672mb)
MSN - 467,075,504 bytes (445mb)
Sogou - 979,653,808 bytes (934mb)

<h2>Running the Code from the Command Line</h2>
You can start an experiment as follows:

sog 2 ntb 2008-06-01 200 200

(...more info coming!)

<h2>Evaluation</h2>

(What the code does - how the paper compares - ...more info coming!)

<h2>Further Information</h2>

If you have any questions on the code or approaches, then visit my website at http://www.stewh.com to find out how to contact me.

