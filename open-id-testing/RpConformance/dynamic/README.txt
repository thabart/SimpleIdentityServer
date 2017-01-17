This README is an appendix to the documentation at http://openid.net/certification/rp_submission/ 
specifically for the oyoidc RP software to explain how this RP meets the submission
criteria and how the results can be reproduced.

Certification for SimpleIdentityServer is done by running an exe that mimics an integration
of the library into a backend service of a website. This test suite is hosted on Github.

The RP certification test results can be reproduced and verified as follows:

1. Clone the simpleidentity server repository github
  git clone https://github.com/thabart/SimpleIdentityServer.git
  open the solution RpConformance.sln

2. Set "RpConformance" and "SimpleIdentityServer.Client" as startup projects

3. Launch the solution.

4. If everything worked you should get the logs in the sub folder "Logs".
 

