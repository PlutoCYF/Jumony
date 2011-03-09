﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using System.Web;
using System.Web.Hosting;
using System.Web.Compilation;

namespace Ivony.Html.Web.Mvc
{
  public class JumonyView : IView
  {

    protected string VirtualPath
    {
      get;
      private set;
    }


    protected IHtmlDocument Document
    {
      get;
      private set;
    }


    public JumonyView( string virtualPath, IHtmlDocument document )
    {
      VirtualPath = virtualPath;
      Document = document;
    }



    public void Render( ViewContext viewContext, TextWriter writer )
    {
      ViewContext = viewContext;
      Render( writer );

      var strings = new string[0];
      object[] objects = strings;

    }


    public ViewContext ViewContext
    {
      get;
      private set;
    }

    public ViewDataDictionary ViewData
    {
      get { return ViewContext.ViewData; }
    }

    public object Model
    {
      get { return ViewData.Model; }
    }




    protected virtual void Render( TextWriter output )
    {
      ProcessDocument();

      ProcessActionLinks();

      AddGeneratorMetaData();


      Document.ResolveUriToAbsoluate();

      Document.Render( output );
    }



    protected virtual void ProcessDocument()
    {

      IHtmlHandler handler = FindHandler();

      if ( handler != null )
        handler.ProcessDocument( ViewContext.HttpContext, Document );

    }

    private IHtmlHandler FindHandler()
    {
      var handlerPath = VirtualPath + ".ashx";

      if ( HostingEnvironment.VirtualPathProvider.FileExists( handlerPath ) )
      {
        try
        {
          return BuildManager.CreateInstanceFromVirtualPath( handlerPath, typeof( JumonyHandler ) ) as JumonyHandler;
        }
        catch { }
      }

      return null;
    }



    protected void ProcessActionLinks()
    {

      var links = Document.Find( "a[action]" );

      foreach ( var actionLink in links )
      {
        var action = actionLink.Attribute( "action" ).Value();
        var controller = actionLink.Attribute( "controller" ).Value();

        var routeValues = new RouteValueDictionary();

        foreach ( var attribute in actionLink.Attributes().Where( a => a.Name.StartsWith( "_" ) ) )
        {
          routeValues.Add( attribute.Name.Substring( 1 ), attribute.Value() );
          attribute.Remove();
        }



        actionLink.Attribute( "action" ).Remove();

        var controllerAttribute = actionLink.Attribute( "controller" );
        if ( controllerAttribute != null )
          controllerAttribute.Remove();


        var href = UrlHelper.GenerateUrl( null, action, controller, routeValues, RouteTable.Routes, ViewContext.RequestContext, true );

        actionLink.SetAttribute( "href", href );
      }

    }



    private void AddGeneratorMetaData()
    {
      var modifier = Document.DomModifier;
      if ( modifier != null )
      {
        var header = Document.Find( "html head" ).FirstOrDefault();

        if ( header != null )
        {

          var metaElement = modifier.AddElement( header, "meta" );

          metaElement.SetAttribute( "name", "generator" );
          metaElement.SetAttribute( "content", "Jumony" );
        }
      }
    }


  }
}
