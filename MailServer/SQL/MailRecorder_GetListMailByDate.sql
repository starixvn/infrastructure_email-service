ALTER PROCEDURE [dbo].[MailRecorder_GetListMailByDate_xtest]		
	@AppID int,
	@FromDate datetime ,
	@ToDate datetime,
	@Status int = 0,
	@Page int = 1,
	@Counter int = 100,
	@ErrCode int = 0 Output,
	@ErrMsg nvarchar(200) = '' Output
	
AS
/*
Created: 09/20/2012@Ricky
Task : get mail from date to date
DB: DB06.bodb_Maintenance
Original:
	
Param's Explanation:

*/
DECLARE @searchSuccess int, @serchfail int
Set @searchSuccess = 0
Set @serchfail = 0
if @Status = 1
	Begin
		set @serchfail = 0
		set @searchSuccess = 1
		
	end
else
	begin
		 if @Status = -1
				begin
					 set @serchfail = 1
					 set @searchSuccess = 0
				end
	end

SET NOCOUNT ON
IF @AppID = 0
	BEGIN
		with MailData as( 
			SELECT ROW_NUMBER() OVER (ORDER BY MailID DESC)
				AS Row, [MailID], [AppID],[FuncID], [MailTo],[Title], [Subject], Body, [MailCc], [MailBcc], [MailDate], [Attachments],[CountSend],[SentSuccess],[SentFail], [MailUpdatedDate]
			FROM [dbo].[Mail_Record]  WITH (NOLOCK)
			WHERE MailDate BETWEEN @FromDate AND @ToDate 
			AND SentSuccess >= @searchSuccess 
			AND SentFail >= @serchfail 
		),

		 MailDataPaged as (SELECT *
		FROM MailData  WITH (NOLOCK)
		WHERE Row between 
		(@Page - 1) * @Counter + 1 and @Page * @Counter),

		 MailDataFunction as (select mp.[MailID], mp.AppID, mp.[MailTo],mp.[Title], mp.[Subject], mp.Body, mp.[MailCc], mp.[MailBcc], mp.[MailDate], mp.[Attachments],mp.[CountSend],mp.[SentSuccess],mp.[SentFail], mp.[MailUpdatedDate],mf.[FuncID], mf.FuncName 
		from MailDataPaged as mp WITH (NOLOCK)
		LEFT JOIN Mail_Function as mf on mp.FuncID = mf.FuncID),

		MailDataFunctionConfig as (select mp.[MailID], mp.AppID, mp.[MailTo],mp.[Title], mp.[Subject], mp.Body, mp.[MailCc], mp.[MailBcc], mp.[MailDate], mp.[Attachments],mp.[CountSend],mp.[SentSuccess],mp.[SentFail], mp.[MailUpdatedDate],mp.[FuncID], mp.FuncName, mc.AppName, mc.SettingID
		from MailDataFunction as mp WITH (NOLOCK)
		Left JOIN Mail_Config as mc ON mc.AppID = mp.AppID)
		
		select mp.[MailID], mp.AppID, mp.[MailTo],mp.[Title], mp.[Subject], mp.[MailCc], mp.[MailBcc], mp.Body, mp.[MailDate], mp.[Attachments],mp.[CountSend],mp.[SentSuccess],mp.[SentFail], mp.[MailUpdatedDate],mp.[FuncID], mp.FuncName, mp.AppName, mp.SettingID, ms.SettingName, ms.IsAuth, ms.Username, ms.Password, ms.Server, ms.Port
		from MailDataFunctionConfig as mp WITH (NOLOCK)
		Left JOIN Mail_Setting as ms ON ms.SettingID = mp.SettingID
	END
ELSE
	BEGIN 
		WITH MailData AS ( 
			SELECT ROW_NUMBER() OVER (ORDER BY MailID DESC)
				AS Row, [MailID], [AppID],[FuncID], [MailTo],[Title], [Subject], Body, [MailCc], [MailBcc], [MailDate], [Attachments],[CountSend],[SentSuccess],[SentFail], [MailUpdatedDate]
			FROM [dbo].[Mail_Record]  WITH (NOLOCK)
			WHERE AppID=@AppID		
			AND MailDate BETWEEN @FromDate AND @ToDate AND SentSuccess >= @searchSuccess AND SentFail >= @serchfail 
		),
		
		MailDataPaged as (SELECT *
		FROM MailData WITH (NOLOCK)
		WHERE Row between 
		(@Page - 1) * @Counter + 1 and @Page * @Counter),
		
		MailDataFunction as (select mp.[MailID], mp.AppID, mp.[MailTo],mp.[Title], mp.[Subject], mp.Body, mp.[MailCc], mp.[MailBcc], mp.[MailDate], mp.[Attachments],mp.[CountSend],mp.[SentSuccess],mp.[SentFail], mp.[MailUpdatedDate],mf.[FuncID], mf.FuncName 
		from MailDataPaged as mp WITH (NOLOCK)
		LEFT JOIN Mail_Function as mf on mp.FuncID = mf.FuncID),

		MailDataFunctionConfig as (select mp.[MailID], mp.AppID, mp.[MailTo],mp.[Title], mp.[Subject], mp.Body, mp.[MailCc], mp.[MailBcc], mp.[MailDate], mp.[Attachments],mp.[CountSend],mp.[SentSuccess],mp.[SentFail], mp.[MailUpdatedDate],mp.[FuncID], mp.FuncName, mc.AppName, mc.SettingID
		from MailDataFunction as mp WITH (NOLOCK)
		Left JOIN Mail_Config as mc ON mc.AppID = mp.AppID)
		
		select mp.[MailID], mp.AppID, mp.[MailTo],mp.[Title], mp.[Subject], mp.[MailCc], mp.[MailBcc],mp.Body, mp.[MailDate], mp.[Attachments],mp.[CountSend],mp.[SentSuccess],mp.[SentFail], mp.[MailUpdatedDate],mp.[FuncID], mp.FuncName, mp.AppName, mp.SettingID, ms.SettingName, ms.IsAuth, ms.Username, ms.Password, ms.Server, ms.Port
		from MailDataFunctionConfig as mp WITH (NOLOCK)
		Left JOIN Mail_Setting as ms ON ms.SettingID = mp.SettingID
	END 


