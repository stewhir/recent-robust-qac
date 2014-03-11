recent-robust-qac
=================

This git repo contains the accompanying query auto-completion (QAC) approach implementations detailed in the paper "Recent and Robust Query Auto-completion" (Whiting &amp; Jose), presented at the World Wide Web Conference (WWW) 2014 in Seoul/South Korea. 

The central idea of this paper is simply: can QAC systems be optimised to support both always popular (i.e. robustness), and recently popular queries? You can read the full paper at: http://www.stewh.com/wp-content/uploads/2014/02/fp539-whiting.pdf. It is written to be accessible to as many readers as possible. Further elaboration on some of the techniques will be included in my thesis - whenever it is finished!

This source code will be helpful to researchers looking to re-implement and improve on these baseline datasets and approaches. Furthermore, practitioners might find some inspiration for implementing their own QAC systems - though bear in mind the objective of this code is for research, so it is far from optimal for production use!

For those wanting to reproduce the experiments in the paper (i.e. to use as a baseline), you will need to prepare the 'typed query' datasets from the raw AOL/MSN/Sogou query logs, using the code contained in the 'Extract*' projects.

The size of each typed query log files is as follows:
AOL - 705,244,335 bytes (672mb)
MSN - 467,075,504 bytes (445mb)
Sogou - 979,653,808 bytes (934mb)




If you have any questions on the code, or wish to read the paper, then visit my website at http://www.stewh.com.

