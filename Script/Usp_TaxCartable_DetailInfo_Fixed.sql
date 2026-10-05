USE [RWP_Sale_DB]
GO
/****** Object:  StoredProcedure [dbo].[Usp_TaxCartable_DetailInfo]    Script Date: 11/5/2024 10:37:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 


ALTER PROCEDURE [dbo].[Usp_TaxCartable_DetailInfo]  
		(  @InvoiceID UNIQUEIDENTIFIER
		  , @BookerStoreID INT 
		  , @BookerWorkstationID INT )
	AS 
	BEGIN

		SET NOCOUNT ON;

		DECLARE   @DateNow		 DATETIME 
				, @InvocieDate   DATETIME 
				, @DocumentDate  DATETIME 
				, @TissInvoiceDate		NVARCHAR (50)
				, @CalcTissInvoiceDate  DATETIME 
				, @CalcTissDocmentDate  DATETIME 
				, @TissDocumentDate		NVARCHAR (50)
		--		, @InvoiceId  uniqueidentifier = 'd7681c8a-c715-44bc-ad1a-9e411d523c06'

			
		SET @DateNow = GETDATE()
		SET @InvocieDate  = ( SELECT  TOP 1 InvoiceDate  FROM SaleInvoice WHERE InvoiceID = @InvoiceId  )	
		SET @InvocieDate  = (SELECT  TOP 1  FORMAT(CONVERT(DATETIME,  InvoiceDate  ), 'yyyy-MM-ddTHH:mm:ssZ') FROM SaleInvoice WHERE InvoiceID = @InvoiceId  )  	
		SET @DocumentDate = ( SELECT  TOP 1 DocumentDate FROM StockDocument WHERE DocumentID = @InvoiceId   )

SET @CalcTissInvoiceDate  = (SELECT  TOP 1    DATEADD ( HOUR   ,-3 , InvoiceDate )    FROM SaleInvoice WHERE InvoiceID = @InvoiceId  )  		
		SET  @TissInvoiceDate  = ( CONVERT (NVARCHAR(500 ),  CONVERT(bigint,( DATEDIFF(SECOND, '1970-01-01',   DATEADD( MINUTE ,      -30 , @CalcTissInvoiceDate	)	   )) )*1000  )   )  	


		SET @CalcTissDocmentDate  = (SELECT  TOP 1    DATEADD ( HOUR ,  -3 , DocumentDate )    FROM StockDocument WHERE DocumentID = @InvoiceId  ) 		
		SET @TissDocumentDate  = ( CONVERT (NVARCHAR(500 ), CONVERT(bigint,( DATEDIFF(SECOND, '1970-01-01',   DATEADD( MINUTE ,    -30 , @CalcTissDocmentDate	)	   )) )*1000  )  )  	


	 

		IF OBJECT_ID('TempDB..#SendInfo' )  IS NOT NULL DROP TABLE #SendInfo
		IF  OBJECT_ID('TempDB..#DetailInfo' ) IS NOT NULL DROP TABLE #DetailInfo



	;WITH Result  
		 AS  
		 (  
		 SELECT     
		    SILI.InvoiceID      
		   , SILI.LineItemID   
		   ,   SILI.ItemID  
		      ,  [vra] =  D_3.TaxAmount + D_3.TollAmount      
		   ,  SILI.ManualDiscount   
		         
		   , PriceAmount   
		   ,Quantity=  Quantity   + BonusQuantity
		   , TotalDiscount   = ISNULL (DiscountAmount ,0 ) + ISNULL (InvoiceDiscount,0 ) + ISNULL (ManualDiscount,0 ) 
		  
		   , TotalDiscountPerLine =CASE WHEN (Quantity   + BonusQuantity)   > 0  
		           THEN ( ISNULL (DiscountAmount ,0 ) + ISNULL (InvoiceDiscount,0 )+ ISNULL (ManualDiscount,0 )  ) / (Quantity   + BonusQuantity)   
		          ELSE 0  
		          END      
		   , SI.ExpectedAmount 
		   , SILI.IsPriceWithTax
		   , D_3_TaxAmount  = D_3  .TaxAmount 
		   , D_3_TollAmount = D_3 .TollAmount    
		   , SILI.TypeID 
		
		  
		 FROM SaleInvoice SI  WITH (NOLOCK )  
		  INNER JOIN SaleInvoiceLineItem SILI WITH (NOLOCK )  
			    ON SI.InvoiceID = SILI.InvoiceID  
		
		  INNER JOIN ItemDepartmentAssignmentView IDA_3  
		   ON IDA_3.ItemID = SILI.ItemID   
		   AND IDA_3.TypeID = 3   
		
		  INNER JOIN Department D_3    
		   ON D_3 .DepartmentID =  IDA_3.DepartmentID   
			 
		 WHERE  InvoiceTypeID IS NULL 
			AND  SI.InvoiceID= @InvoiceID   
			AND SI.BookerStoreID = @BookerStoreID
			AND SI.BookerWorkstationID = @BookerWorkstationID
  )  
    
 , Result_2 AS   
    (  
  
   SELECT      
      *  
      , TotalDiscountPercent = CASE WHEN PriceAmount  > 0   
              THEN ( TotalDiscountPerLine / PriceAmount   ) * 100   
            ELSE 0   
            END   
  
        
  
   FROM Result  R   
    ) 

 , Result_3
 AS  (
 SELECT 
				
	    r.ExpectedAmount
	  ,	r.[InvoiceID]	  
	  ,	r.TypeID	  
      , r.[LineItemID]
      , r.[ItemID]	  
      , r.[vra]	  
      , r.[ManualDiscount]            
      ,r.[PriceAmount]
      ,r.[Quantity]
      ,r.[TotalDiscount]
      ,r.[TotalDiscountPerLine]      
      ,r.[TotalDiscountPercent]            
      , [vop]       = 0 

		, fee=  ROUND(  
								CAST ( CASE WHEN IsPriceWithTax  = 1                    
												THEN r.PriceAmount /  ( ( ( D_3_TaxAmount + D_3_TollAmount ) / 100 ) +1 )              
											ELSE r.PriceAmount        
											END   AS DECIMAL( 18 ,8 )    
										) 										
						 ,0  )  
		
		
		FROM Result_2 R
 
	)
	
	, Result_4 
		AS (
		SELECT R.* 
		
     				, [dis] = FLOOR ( 
						      ( ROUND ( (  fee  *    Quantity) , 0,1 )  * TotalDiscountPercent) /100
						  )

	  ,   [adis] = FLOOR (   ROUND( ( fee * Quantity ) , 0,1) - FLOOR( (( ROUND ( (  fee  *    Quantity) , 0 ,1 )  * TotalDiscountPercent) /100))
					     )
		      


		FROM Result_3 R
		)
			SELECT 
			 
					*  							
				,   vam =  floor ( ( [vra] / 100 )  *  adis  )
				,  [NewPayAmount] = ( [adis]   +  floor ( ( [vra] / 100 )  *  adis   )  )
				 , Ver = '3.0.0.0'
							
								INTO #DetailInfo 
			
			FROM Result_4 R 



  SELECT         

  /*---------------------------------------HeaderInfo ----------------------------------------------------*/        
 
   SI.BookerStoreID  
  ,  SI.BookerWorkstationID  
  ,  SI.InvoiceID 

   , BaranTypeId = TI.TypeID
  ,  TI.ItemID       
     ,  bid    = NULL       --شناسه ملی خریدار        
     ,  bpc    = NULL   -- کد پستی خریدار برای حقیق ها اجباری        
     ,  bbc    = NULL        
     ,  bpn    = NULL         
     ,  ft     = NULL         
     ,  scln   = NULL   
     ,  scc    = NULL         
     ,  crn    = NULL         
     , cdcn   = NULL       
    ,  cdcd   = NULL         
     ,  billid = NULL                             
     ,  todam  = NULL       
     ,  tonw   = NULL        
     ,  torv   = NULL        
     ,  tocv   = NULL        
     ,  setm   =  1    --نوع پرداخت 1 نقدی 2 نسیه 3 نقدی نسیه            
     ,  insp   =  NULL -- مبلغ نسیه        
     ,  tvop   = NULL        
     ,  taxid = ''    -- پاس داده شود سرویس ایجاد می کند Null         
     ,  indatim =ISNULL (@TissInvoiceDate , @TissDocumentDate  )
	 --CONCAT( CAST ( SI.InvoiceDate AS DATE  ),'T' , FORMAT(SI.InvoiceDate, 'hh\:mm\:ss')  , 'Z' )   -- تاریخ صدور صورتحساب        
        
     ,  inty  = 2 --نوع صورتحساب ، نوع یک آن ها صاحب کارپوشه هستند        
          -- نوع 2 مصرف کننده نهایی - اشخاص عادی         
    -- inno در قرارداد برنامه BIGINT است. TRY_CONVERT مانع شکست کل نتیجه
    -- در صورت وجود داده ناسالم یا شناسه 22 کاراکتری در TaxInnoNumber می‌شود.
    , inno = TRY_CONVERT
             (
                 BIGINT,
                 NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(100), SCF.TaxInnoNumber))), N'')
             )
    -- irtaxid یک شناسه متنی است؛ برای صورتحساب اصلی رشته خالی ارسال نمی‌شود.
    , irtaxid = CONVERT
                (
                    NVARCHAR(50),
                    CASE
                        WHEN TI.TypeID = 302 THEN NULL
                        ELSE NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(50), SCF.IrtaxID))), N'')
                    END
                )
    , inp  = 1        

    , ins = CASE WHEN  SI.ExpectedAmount < 0 
						THEN 4
					ELSE 1
					END     
        
     , tins ='10100812474'--  شناسه ملی فروشنده      --NeedChange   
     , tinb = null--  شماره اقتصادی خریدار        
     , tob= CASE WHEN IsIndividual = 1         
					THEN 1         
			   WHEN  IsIndividual = 0  THEN 2        
			   END 
           
        
    , Tax17 = null         
    , Indati2m  = null        
        
 /*---------------------------------------DetailInfo ----------------------------------------------------*/        
        
     , sstid = ICF.CommodityID   --شناسه کالا و خدمات        
    ,  sstt =   I.Name    
    ,  mu =null  -- واحد اندازه گیری        
    ,  [am]     =  TI.Quantity        
    ,  nw  = NULL         
    ,  fee = fee  
    ,  [cfee]  =    NULL         
    ,  [cut]   =    NULL        
    ,  [exr]   =    NULL        
    ,  [ssrv] =     NULL        
    ,  [sscv] =    NULL        
    ,  [prdis] =  ROUND (  fee * Quantity  ,0 ,1)
    ,  [dis] =  dis  
    ,  [adis] = adis  
    ,  [vra] =  vra  
    ,  [vam]  = TI.vam          
    ,  [odt] = NULL         
    ,  [odr] = NULL         
    ,  [odam] = NULL         
    ,  [olt]  = NULL         
    ,  [olr] = NULL         
    ,  [sbc] =NULL   
    ,  [olam] = NULL        
    ,  [consfee] = NULL         
    ,  [spro] = NULL         
    ,  [bros] = NULL      
    ,  [tcpbs] = NULL         
    ,  [cop] = NULL         
    ,  [vop] = NULL
 ,  [bsrn] = NULL         
    ,  [tsstam] =NewPayAmount   
          
 /*------------------------------------------Payment ----------------------------------------------------*/        
  
    , [iinn] = NULL         
    , [acn] = NULL         
    , [trmn] = NULL         
    , [pmt] =NULL         
    , [trn]=NULL         
    , [pcn]=NULL         
    , [pid]=NULL         
    , [pdt]=NULL         
    , [pv] = NULL     
	, SI.InvoiceTypeID 
	, SI.InvoiceDate
	, CV.CustomerName
	, SI.BusinessDate
	, SI.InvoiceNumber 
	, tprdis =  SUM( ROUND( fee * Quantity , 0,1)  ) OVER  (PARTITION BY  SI.InvoiceID  )                
   ,  tdis =    SUM( dis    )						 OVER  (PARTITION BY  SI.InvoiceID  )                  
   ,  tadis =   SUM( TI.adis  )						 OVER  (PARTITION BY  SI.InvoiceID  )                      	
   ,  tvam  =   SUM ( vam )							 OVER  (PARTITION BY  SI.InvoiceID  )                
   ,  tbill  =  SUM (TI.NewPayAmount )				 OVER  (PARTITION BY  SI.InvoiceID  )                
   ,  cap   =   SUM (TI.NewPayAmount )				 OVER  (PARTITION BY  SI.InvoiceID  ) 
   
	, Ver = '3.0.0.0'
        
 /*----------------------------------------------------------------------------------------------------------*/        
        
 FROM SaleInvoice SI   WITH (NOLOCK)         

	INNER JOIN #DetailInfo TI  
		ON  TI.InvoiceID = SI.InvoiceID 
		
	INNER JOIN Item I      WITH (NOLOCK)          
		ON I.ItemID = TI.ItemID  

	INNER JOIN ItemCustomField ICF   WITH (NOLOCK)      
			ON ICF.ItemID = I.ItemID   

	LEFT JOIN SaleInvoiceCustomField SCF  WITH (NOLOCK )  
		ON SCF.InvoiceID = SI.InvoiceID 
 
        
	LEFT JOIN CustomerView CV         
		 ON CV.CustomerID = SI.CustomerID         
		AND CV.LanguageID = 314         
   
 
            
      UNION  ALL         
	  
SELECT 
		BookerStoreID
		,BookerWorkstationID
		,DocumentID
		, TypeID
		,ItemID		
		,bid
		,bpc
		,bbc
		,bpn
		,ft
		,scln
		,scc
		,crn
		,cdcn
		,cdcd
		,billid		
		,todam		
		,tonw
		,torv
		,tocv
		,setm
		,insp
		,tvop
		,taxid
		,indatim
		,inty
		,inno = TRY_CONVERT(BIGINT, TB.inno)
		,irtaxid = CONVERT
		            (
		                NVARCHAR(50),
		                NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(50), TB.irtaxid))), N'')
		            )
		,inp
		,ins
		,tins
		,tinb
		,tob
		,Tax17
		,Indati2m
		,sstid
		,sstt
		,mu
		,am
		,nw
		,fee
		,cfee
		,cut
		,exr
		,ssrv
		,sscv
		,prdis
		,dis
		,adis
		,vra
		,vam
		,odt
		,odr
		,odam
		,olt
		,olr
		,sbc
		,olam
		,consfee
		,spro
		,bros
		,tcpbs
		,cop
		,vop
		,bsrn
		,tsstam
		,iinn
		,acn
		,trmn
		,pmt
		,trn
		,pcn
		,pid
		,pdt
		,pv
		,DocumentTypeID
		,DocumentDate
		,CustomerName
		,BusinessDate
		, DocumentCode
		, tprdis
		, tdis 
		, tadis 		
		, tvam  
		, tbill  
		,  cap   
		, Ver = '3.0.0.0'
			
		
		FROM [UV_TaxInfo_BulkSale] TB
		WHERE   TB.DocumentID= @InvoiceID  					
			AND TB.BookerStoreID = @BookerStoreID
			AND TB.BookerWorkstationID = @BookerWorkstationID





				--SELECT *  FROM SaleInvoice WHERE InvoiceNumber ='2-19-3043945'

	END;
GO
