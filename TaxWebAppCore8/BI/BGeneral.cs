using NPSDTOModels.DBDTO.Request;
using ClsTools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TaxServiceAndApp.Models;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;

namespace TaxServiceAndApp.BI
{
    public static class BGeneral
    {
        public static async Task<DTOEntityComment> GetEntityCommentByIDAsync(Guid ID)
        {
            try
            {
                DTOEntityComment dtentityComment = new DTOEntityComment();
                using (NPSGeneralContext ef = new NPSGeneralContext())
                {
                    ef.Database.Initialize(force: true);
                    var vwcomments = await ef.tEntityComments
                        .ToListAsync();

                    List<Guid> Ids = new List<Guid>();

                    var vwcomment = ef.tEntityComments
       .FirstOrDefault(a => a.fCommentUniqueID == ID);
                    vwcomment = vwcomments.FirstOrDefault(a => a.fCommentUniqueID == ID);
                    if (vwcomment != null)
                        dtentityComment = ObjectMapper.TargetObject<DTOEntityComment>(vwcomment);

                }
                return dtentityComment;

            }
            catch (Exception e)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).LogError("- GetEntityCommentByEntityID Exception - " + e.Message, e);
                return null;
            }
        }
        public static async Task<List<DTOEntityComment>> DeleteEntityCommentByIDAsync(Guid ID)
        {
            try
            {
                DTOEntityComment dtentityComment = new DTOEntityComment();
                using (NPSGeneralContext ef = new NPSGeneralContext())
                {
                    ef.Database.Initialize(force: true);
                    var vwcomments = await ef.tEntityComments
                        .ToListAsync();

                    List<Guid> Ids = new List<Guid>();

                    var vwcomment = ef.tEntityComments
       .FirstOrDefault(a => a.fCommentUniqueID == ID);
                    vwcomment = vwcomments.FirstOrDefault(a => a.fCommentUniqueID == ID);
                    if (vwcomment != null)
                        vwcomment.fDeleted = true;

                    ef.tEntityComments.AddOrUpdate(a => a.fID, vwcomment);
                    await ef.SaveChangesAsync();
                }
                return await GetEntityCommentByUserIDAsync();

            }
            catch (Exception e)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).LogError("- GetEntityCommentByEntityID Exception - " + e.Message, e);
                return null;
            }
        }
        public static async Task<List<DTOEntityComment>> GetEntityCommentByUserIDAsync(Guid PersonUID = default(Guid))
        {
            try
            {
                List<DTOEntityComment> dtentityComment = new List<DTOEntityComment>();
                using (NPSGeneralContext ef = new NPSGeneralContext())
                {

                    ef.Database.Initialize(force: true);
                    var vwcomments = await ef.tEntityComments
                        .Where(a =>a.fDeleted!=true&&( PersonUID == default(Guid) || a.fPersonUniqueID == PersonUID)).ToListAsync();

                    foreach (var item in vwcomments)
                    {
                        dtentityComment.Add(ObjectMapper.TargetObject<DTOEntityComment>(item));
                    }
                }
                return dtentityComment;
            }
            catch (Exception e)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).LogError("- GetEntityCommentByEntityID Exception - " + e.Message, e);
                return null;
            }
        }
        public static async Task<List<DTOEntityComment>> AddEntityCommentAsync(DTOEntityComment comment)
        {
            try
            {
                using (NPSGeneralContext ef = new NPSGeneralContext())
                {
                    ef.Database.Initialize(force: true);
                    if (comment.fCommentUniqueID == default(Guid))
                    {
                        comment.fCommentUniqueID = ClsTools.ClsTools.CreateUniqueID();
                        comment.fCreatedDate = DateTime.Now;
                    }
                    var tcom = ObjectMapper.TargetObject<tEntityComments>(comment);

                    ef.tEntityComments.AddOrUpdate(a => a.fCommentUniqueID, tcom);

                    await ef.SaveChangesAsync();
                }
                var commensts = await GetEntityCommentByUserIDAsync();
                return commensts;

            }
            catch (Exception e)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).LogError("- AddEntityComment Exception - " + e.Message, e);
                return null;
            }
        }
    }
}