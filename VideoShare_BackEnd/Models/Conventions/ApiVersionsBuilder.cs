using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using VideoShare_BackEnd.Models.Attributes;
using VideoShare_BackEnd.Utils.NullUtils;

namespace VideoShare_BackEnd.Models.Conventions
{
    public class ApiVersionsBuilder : ApiVersionConventionBuilder
    {
        public override bool ApplyTo(ControllerModel controller)
        {
            var builder = this.Controller(controller.ControllerType);
            SetControllerApiVersions(builder, controller);

            for (int i = 0; i < controller.Actions.Count; i++)
            {
                SetActionMapToVersions(builder, controller.Actions[i]);
            }

            return base.ApplyTo(controller);
        }

        private void SetControllerApiVersions(IControllerConventionBuilder builder, ControllerModel controller)
        {
            //AllApiVersionsAttribute
            var supportAllVersions = controller.Attributes.OfType<AllApiVersionsAttribute>().Any();
            if (supportAllVersions)
            {
                for (var i = 0; i < App.ApiVersions.Count; i++)
                {
                    builder.HasApiVersion(new ApiVersion(App.ApiVersions[i]));
                }
            }
            else
            {
                //ApiVersionsAttribute
                var versions = controller.Attributes.OfType<ApiVersionsAttribute>().Select(a => a.versions).FirstOrDefault();
                if (!versions.IsNullOrEmpty())
                {
                    for (var i = 0; i < versions.Length; i++)
                    {
                        builder.HasApiVersion(new ApiVersion(versions[i]));
                    }

                    return;
                }

                //ApiVersionToLatestAttribute
                var ver = controller.Attributes.OfType<ApiVersionToLatestAttribute>().Select(a => a.version).FirstOrDefault();
                if (ver is not null)
                {
                    for (var i = 0; i < App.ApiVersions.Count; i++)
                    {
                        if (App.ApiVersions[i] >= ver)
                        {
                            builder.HasApiVersion(new ApiVersion(App.ApiVersions[i]));
                        }
                    }
                }
                
            }
        }

        private void SetActionMapToVersions(IControllerConventionBuilder controllerbuilder, ActionModel action)
        {
            var versions = action.Attributes.OfType<ApiVersionsAttribute>().Select(a => a.versions).FirstOrDefault();
            if (!versions.IsNullOrEmpty())
            {
                var builder = controllerbuilder.Action(action.ActionName);
                for (var i = 0; i < versions.Length; i++)
                {
                    builder.HasApiVersion(new ApiVersion(versions[i]));
                }
            }

            var ver = action.Attributes.OfType<ApiVersionToLatestAttribute>().Select(a => a.version).FirstOrDefault();
            if (ver is not null)
            {
                var builder = controllerbuilder.Action(action.ActionName);
                for (var i = 0; i < App.ApiVersions.Count; i++)
                {
                    if (App.ApiVersions[i] >= ver)
                    {
                        builder.HasApiVersion(new ApiVersion(App.ApiVersions[i]));
                    }
                }
            }
        }
    }
}