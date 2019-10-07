

This chapter will be about using Layout pages (Razor pages) with materialize css to create a consistent look for our website.

Layout pages are used in web applications so we can create the "look" of our webpage in one place, and then use that same "look"
with all of our views.  A view (Index.cshtml, About.cshtml, Contact.cshtml, etc) will use the _Layout.cshtml file by default in
an ASP.Net application. 

However, it is uncommon to just have one layout page for a full web application.  Different "areas" of a web application will have
different looks. For example, the home page of a web site will look much different than your account profile page. However, all the different
pages associated with your account profile will look the same. There is a layout for the home page (and usually many pages) that is used
for that area of the web application, however, a different layout is used for the account profile pages.

In this chapter, we will create a "_MasterLayout.cshtml"(Razor page) layout that all other layouts will use. We will update the _Layout.cshtml file to use the 
new _MasterLayout.cshtml page we created. We will also update the _Layout.cshtml page to use the materialize css.  We will also create a different
layout called _SecureLayout.cshtml that will be used for the "dashboard" and account profile area of the web application. Finally we will create a
Dashboard.cshtml that will use the _SecureLayout.cshtml page that we created.

