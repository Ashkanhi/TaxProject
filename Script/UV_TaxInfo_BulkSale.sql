

   
    
          
 CREATE VIEW   [dbo].[UV_TaxInfo_BulkSale]       
   AS           
  
   WITH     
      Cte_Irtaxid AS           
    (          
            
     SELECT   
     SD.DocumentID   
   , SD.DocumentDate       
   , ( REPLACE ( SD.DocumentCode  , '-', '' )  )  AS CreateIrtaxid             
      
      FROM StockDocument SD   
   WHERE   SD.DocumentTypeID IN ( 395 , 397 )            
   AND SD.StatusID = 246   
     )          
            
          
  SELECT           
      
  /*---------------------------------------HeaderInfo ----------------------------------------------------*/          
  --   id = si.InvoiceID    
  SD.BookerStoreID    
     ,  SD.BookerWorkstationID    
  ,  SD.DocumentID    
   , SD.DocumentTypeID TypeID  
     ,  SDLI.ItemID    
     ,  cap    = SUM(( ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 ) - Discount ) + ( Tax + Toll  ) )  OVER (PARTITION BY SD.DocumentID  )     -- مبلغ قابل پرداخت نقد          
     ,  bid    = NULL    --شناسه ملی خریدار          
     ,  bpc    = NULL    -- کد پستی خریدار برای حقیق ها اجباری          
     ,  bbc    = NULL          
     ,  bpn    = NULL           
     ,  ft     = NULL           
     ,  scln   = NULL     
     ,  scc    = NULL           
     ,  crn    = NULL           
     ,  cdcn   = NULL         
     ,  cdcd   = NULL           
     ,  billid = NULL    
     ,  tprdis =  SUM( ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 )) OVER (PARTITION BY SD.DocumentID  )  -- ه ازای کل فاکتور مبلغ قبل از تخفیف          
     ,  tdis   =  SUM( ROUND (  Discount  ,0 )) OVER (PARTITION BY SD.DocumentID  )          
     ,  tadis  =  SUM( ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 ) - Discount ) OVER (PARTITION BY SD.DocumentID  )  -- مجموع مبلغ کل بعد از تفیفات          
     ,  tvam   = SUM( Tax + Toll  ) OVER (PARTITION BY SD.DocumentID  )  
     ,  todam  = NULL    
     ,  tbill  = SUM(( ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 ) - Discount ) + ( Tax + Toll  ) )  OVER (PARTITION BY SD.DocumentID  ) -- مجموع کل صورتحساب حتی اگر اقساط          
     ,  tonw   = NULL          
     ,  torv   = NULL          
     ,  tocv   = NULL          
     ,  setm   =  1    --نوع پرداخت 1 نقدی 2 نسیه 3 نقدی نسیه              
     ,  insp   =  NULL -- مبلغ نسیه          
     ,  tvop   = NULL          
     ,  taxid = ''    -- پاس داده شود سرویس ایجاد می کند Null           
     ,  indatim =CONCAT( CAST ( SD.DocumentDate AS DATE  ),'T' , FORMAT(SD.DocumentDate , 'hh\:mm\:ss')  , 'Z' )   -- تاریخ صدور صورتحساب          
          
     ,  inty  = 1 --نوع صورتحساب ، نوع یک آن ها صاحب کارپوشه هستند          
          -- نوع 2 مصرف کننده نهایی - اشخاص عادی           
    ,  inno = CAST ( RIGHT(   CONCAT (SD.DocumentTypeID , RIGHT( SD.DocumentCode , 7) , SD.BookerStoreID , SD.BookerWorkstationID   ),10)  AS BIGINT  )          
  
    , irtaxid =N''      
    , inp  = 1          
    , ins = CASE WHEN SD.DocumentTypeID IN (395)           
     THEN 1          
    WHEN SD.DocumentTypeID IN (303 )           
     THEN 4           
   END          
          
     , tins ='14005809943'--  شناسه ملی فروشنده      --NeedChange     
     , tinb = null--  شماره اقتصادی خریدار          
     , tob= CASE WHEN COALESCE ( CV.IsIndividual, SV.IsIndividual,1   ) = 1           
      THEN 1           
    ELSE 2          
    END           
          
    , Tax17 = null           
    , Indati2m  = null          
          
 /*---------------------------------------DetailInfo ----------------------------------------------------*/          
          
     , sstid = ICF.CommodityID   --شناسه کالا و خدمات          
    , sstt =   I.Name      
    , mu =null  -- واحد اندازه گیری          
    , [am]     =  ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )          
    ,  nw  = null           
    ,  fee = SDLI.SalePrice     
    ,  [cfee]  =    NULL           
    ,  [cut]   =    NULL          
    ,  [exr]   =    NULL          
    ,  [ssrv] =     NULL          
    ,  [sscv] =    NULL          
    ,  [prdis] =  ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 )  
    ,  [dis] =  Discount   
    ,  [adis] = ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 ) - Discount   
    ,  [vra] =  IDA_Type_3.TaxAmount + IDA_Type_3.TollAmount  
    ,  [vam]  = SDLI.Tax + SDLI.Toll   
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
    ,  [vop] = 0  
 ,  [bsrn] = NULL           
    ,  [tsstam] =(ROUND (  SalePrice * ( SDLI.UnitCount + ( SDLI.PackCount * SDLI.PackUnitCount ) )  ,0 ) - Discount )  + (Tax + Toll  )  
            
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
 , SD.DocumentTypeID   
 , SD.DocumentDate  
 , CustomerName = COALESCE (CV.CustomerName , SV.Name, EV.EmployeeName )  
 , SD.BusinessDate   
          
 /*----------------------------------------------------------------------------------------------------------*/          
          
 FROM StockDocument SD   WITH (NOLOCK)        
 INNER JOIN StockDocumentLineItem  SDLI  
  ON  SD.DocumentID  = SDLI.DocumentID  
 LEFT JOIN ThirdPartyView TPV  
  ON TPV.ThirdPartyID = SD.ThirdPartyID   
  AND TPV.LanguageID = 314   
 LEFT JOIN CustomerView CV   
  ON CV.CustomerID = TPV.CustomerID   
  AND CV.LanguageID = 314   
 LEFT JOIN SupplierView SV   
  ON SV.SupplierID = TPV.SupplierID   
  AND SV.LanguageID =314   
 LEFT JOIN EmployeeView EV  
  ON EV.EmployeeID = TPV.EmployeeID   
  AND EV.LanguageID =314   
 INNER JOIN ItemDepartmentAssignmentView IDA_Type_3  
  ON IDA_Type_3.ItemID = SDLI.ItemID  
  AND IDA_Type_3.TypeID = 3   
 INNER JOIN Item I      WITH (NOLOCK)            
  ON I.ItemID = SDLI.ItemID          
 INNER JOIN ItemCustomField ICF   WITH (NOLOCK)        
  ON ICF.ItemID = I.ItemID                                
 LEFT JOIN SaleInvoiceTaxReference TR        
  ON  SD.BookerStoreID = TR.BookerStoreID        
  AND SD.BookerWorkstationID = TR.BookerWorkstationID        
  AND SD.DocumentID = TR.InvoiceID        
   WHERE SD.StatusID = 246   
 AND SD.DocumentTypeID IN (395,397  )  
          
          
          
          
  
GO


