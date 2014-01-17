/* File Created: September 20, 2012 */
set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go



ALTER PROCEDURE [dbo].[MailRecorder_UpdateMailStatus]		
	@MailID bigint,	
	@Status bit,
	@ErrCode int = 0 Output,
	@ErrMsg nvarchar(200) = '' Output
	
AS
/*
Created: 09/20/2012 @Ricky
Task : Update Mail Status
DB: DB06.bodb_Maintenance
Original:
	
Param's Explanation:

*/
UPDATE dbo.Mail_Record with (ROWLOCK, UPDLOCK)
SET SentSuccess = 0
WHERE MailID=@MailID and SentSuccess is NULL

UPDATE dbo.Mail_Record with (ROWLOCK, UPDLOCK)
SET SentFail = 0
WHERE MailID=@MailID and SentFail is NULL

SET NOCOUNT ON
IF @Status > 0
BEGIN
	UPDATE dbo.Mail_Record with (ROWLOCK, UPDLOCK)
	SET SentSuccess = SentSuccess + 1
	WHERE MailID=@MailID
	IF @@Error <> 0
	BEGIN			
		SELECT @ErrMsg = 'Error! Error when updating count mail.', @ErrCode = 101			
		RETURN
	END
END
ELSE
BEGIN
	UPDATE dbo.Mail_Record with (ROWLOCK, UPDLOCK)
	SET SentFail = SentFail + 1
	WHERE MailID=@MailID
	IF @@Error <> 0
	BEGIN			
		SELECT @ErrMsg = 'Error! Error when updating count mail.', @ErrCode = 101			
		RETURN
	END
END











