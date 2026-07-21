 
 

 CREATE  view     [dbo].[UV_TissInfo]
 as         
         
 WITH InvoiceType        
  AS         
   (        
    SELECT         
      SI.InvoiceID        
       , SILI.TypeID                    
        
    FROM SaleInvoice SI         
     INNER JOIN SaleInvoiceLineItem SILI         
      ON SI.InvoiceID = SILI.InvoiceID        
    WHERE SI.InvoiceTypeID IS NULL         
 AND   SI.BusinessDate >= DATEADD (MONTH , -2  , GETDATE())          
    GROUP BY         
      SI.InvoiceID        
       , SILI.TypeID             
   )        
  ,        
   HeaderSaleInfo        
   AS         
   (        
    SELECT  InvoiceID         
  
   , tprdis =  SUM( ROUND( fee * Quantity , 0)  )                
   , tdis =    SUM( dis    )        
   , tadis =   SUM( UT.adis  )        
   , tvop  =   SUM ( UT.vop )   
   , tvam  =   SUM ( vam )  
   , CreateExpected = SUM (ut.NewPayAmount )
        
    FROM Uv_TissInvoice UT       
    GROUP BY InvoiceID         
        
    )        
        
  SELECT         
    
  /*---------------------------------------HeaderInfo ----------------------------------------------------*/        
  --   id = si.InvoiceID  
  SI.BookerStoreID  
     ,  SI.BookerWorkstationID  
  ,  SI.InvoiceID    
  ,  302 AS TypeID  
  ,  TI.ItemID  
     ,  cap    = CreateExpected     -- مبلغ قابل پرداخت نقد        
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
     ,  tprdis = Round( tprdis , 0 )  -- ه ازای کل فاکتور مبلغ قبل از تخفیف        
     ,  tdis   =  tdis        
     , tadis  =  tadis  -- مجموع مبلغ کل بعد از تفیفات        
     ,  tvam   = ISNULL (HS.tvam     , 0   )  
     ,  todam  = NULL  
     ,  tbill  = CreateExpected  -- مجموع کل صورتحساب حتی اگر اقساط        
     ,  tonw   = NULL        
     ,  torv   = NULL        
     ,  tocv   = NULL        
     ,  setm   =  1    --نوع پرداخت 1 نقدی 2 نسیه 3 نقدی نسیه            
     ,  insp   =  NULL -- مبلغ نسیه        
     ,  tvop   = NULL        
     ,  taxid = ''    -- پاس داده شود سرویس ایجاد می کند Null         
     ,  indatim =CONCAT( CAST ( SI.InvoiceDate AS DATE  ),'T' , FORMAT(SI.InvoiceDate, 'hh\:mm\:ss')  , 'Z' )   -- تاریخ صدور صورتحساب        
        
     ,  inty  = 2 --نوع صورتحساب ، نوع یک آن ها صاحب کارپوشه هستند        
          -- نوع 2 مصرف کننده نهایی - اشخاص عادی         
    ,  inno = SCF.TaxInnoNumber       
    , irtaxid = N''         
    , inp  = 1        
    , ins = CASE WHEN IT.TypeID  IN (302 )         
             THEN 1        
         WHEN IT.TypeID  IN (303 )         
            THEN 4         
         WHEN SI.InvoiceTypeID IN ( 360 )         
          THEN 3              
       END        
        
     , tins ='10100812474'--  شناسه ملی فروشنده      --NeedChange   
     , tinb = null--  شماره اقتصادی خریدار        
     , tob= CASE WHEN IsIndividual = 1         
         THEN 1         
       ELSE 2        
 END         
        
    , Tax17 = null         
    , Indati2m  = null        
        
 /*---------------------------------------DetailInfo ----------------------------------------------------*/        
        
     , sstid = ICF.CommodityID   --شناسه کالا و خدمات        
    , sstt =   I.Name    
    , mu =null  -- واحد اندازه گیری        
    , [am]     =  TI.Quantity        
    ,  nw  = null         
    ,  fee = fee  
    ,  [cfee]  =    NULL         
    ,  [cut]   =    NULL        
    ,  [exr]   =    NULL        
    ,  [ssrv] =     NULL        
    ,  [sscv] =    NULL        
    ,  [prdis] =  ROUND (  fee * Quantity  ,0 )
    ,  [dis] =  dis  
    ,  [adis] = adis  
    ,  [vra] =  vra  
    ,  [vam]  = TI.vam          
    ,  [odt] = NULL         
    ,  [odr] = NULL         
    ,  [odam] = NULL         
    ,  [olt]  = ''         
    ,  [olr] = 0         
 ,  [sbc] =NULL   
    ,  [olam] = 0        
    ,  [consfee] = 0         
    ,  [spro] = 0         
    ,  [bros] = 0        
    ,  [tcpbs] = 0         
    ,  [cop] =  0         
    ,  [vop] = ISNULL ( vop   , 0   )  
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
        
 /*----------------------------------------------------------------------------------------------------------*/        
        
 FROM SaleInvoice SI   WITH (NOLOCK)    
 LEFT JOIN SaleInvoiceCustomField SCF 
	ON SCF.InvoiceID = SI.InvoiceID 
 INNER JOIN Uv_TissInvoice TI  
  ON  TI.InvoiceID = SI.InvoiceID        

 INNER JOIN Item I      WITH (NOLOCK)          
  ON I.ItemID = TI.ItemID        

 INNER JOIN ItemCustomField ICF   WITH (NOLOCK)      
  ON ICF.ItemID = I.ItemID       
        
 LEFT JOIN CustomerView CV         
	 ON CV.CustomerID = SI.CustomerID         
	AND CV.LanguageID = 314         
        
 LEFT JOIN HeaderSaleInfo HS         
	ON HS.InvoiceID = SI.InvoiceID         
        
 LEFT JOIN InvoiceType IT         
	  ON IT.InvoiceID = SI.InvoiceID        
      
            
      UNION           
	  
SELECT 
		BookerStoreID
		,BookerWorkstationID
		,DocumentID
		, TypeID
		,ItemID
		,cap
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
		,tprdis
		,tdis
		,tadis
		,tvam
		,todam
		,tbill
		,tonw
		,torv
		,tocv
		,setm
		,insp
		,tvop
		,taxid
		,indatim
		,inty
		,inno
		,irtaxid
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
		
		FROM [UV_TaxInfo_BulkSale]


GO


