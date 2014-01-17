set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go


CREATE PROCEDURE [dbo].[MailRecorder_GetConfig]		
	@AppID int,
	@ErrCode int = 0 Output,
	@ErrMsg nvarchar(200) = '' Output
	
AS
/*
Created: 09/29/2011@Bobby
Task : select mail configuration
DB: DB06.bodb_Maintenance

Revisions:
	- 9/20/2012 Ricky: Updated for new maintenance site
	- 7/29/2013 Ricky: Get Mail from

*/
SET NOCOUNT ON
BEGIN
	SELECT mc.AppID, mc.AppName, mc.Description, mc.MailFrom, ms.SettingID, ms.SettingName, ms.IsAuth, ms.Username, ms.Password, ms.Server, ms.Port
	FROM [dbo].[Mail_Config] as mc WITH(NOLOCK)
	LEFT JOIN Mail_Setting as ms WITH(NOLOCK)
	ON mc.SettingID = ms.SettingID
	WHERE mc.AppID=@AppID 
END

GO
GRANT EXECUTE ON [dbo].[MailRecorder_GetConfig] TO [bodbAdminNet]