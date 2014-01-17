USE [bodb_Maintenance]
GO
set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go


ALTER PROCEDURE [dbo].[MailRecorder_GetListMail]		
	@AppID int,
	@MailDate datetime,
	@Page int = 1,
	@Counter int = 100,
	@ErrCode int = 0 Output,
	@ErrMsg nvarchar(200) = '' Output
	
AS
/*
Created: 09/29/2011@Bobby
Task : select mail configuration
DB: DB06.bodb_Maintenance
Original:
	
Param's Explanation:

*/
SET NOCOUNT ON
BEGIN 
	WITH MailData AS ( 
		SELECT ROW_NUMBER() OVER (ORDER BY MailID DESC)
			AS Row, [MailID], [AppID], [MailTo],[Title], [Subject], [MailCc], [MailBcc], [MailDate], [Attachments]
		FROM [dbo].[Mail_Record]  WITH (NOLOCK)
		WHERE AppID=@AppID
			AND convert(varchar, MailDate, 3)=convert(varchar, @MailDate, 3))

	SELECT *
	FROM MailData 
	WHERE Row between 
	(@Page - 1) * @Counter + 1 and @Page * @Counter
END 
--test
--declare @maildate datetime
--set @maildate = getdate()
--EXEC MailRecorder_GetListMail 2, @maildate

GO
GRANT EXECUTE ON [dbo].[MailRecorder_GetListMail] TO [bodbAdminNet]