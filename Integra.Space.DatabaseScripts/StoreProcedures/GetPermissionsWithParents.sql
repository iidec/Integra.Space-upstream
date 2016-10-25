CREATE PROCEDURE [space].[get_permissions_with_parents]
	@granularPermissionName nvarchar(50),
	@secureClassName nvarchar(50)
AS

WITH Hierarchy(ChildSCId, ChildGPId, ParentSCId, ParentGPId 
				, ChildScName, ChildGpName
				, Parents)
AS
(
    SELECT FirtGeneration.sc_id, FirtGeneration.gp_id,  FirtGeneration.sc_id_parent, FirtGeneration.gp_id_parent
			,sc.sc_name, gp.gp_name			
			, CAST('' AS NVARCHAR(MAX))
        FROM space.hierarchy_permissions AS FirtGeneration
		inner join space.granular_permissions as gp on gp.gp_id = FirtGeneration.gp_id 
		inner join space.[securable_classes] as sc on sc.sc_id = FirtGeneration.sc_id 
        WHERE FirtGeneration.sc_id_parent is null
		or FirtGeneration.gp_id_parent is null
	UNION ALL
    SELECT NextGeneration.sc_id, NextGeneration.gp_id, Parent.ChildSCId, Parent.ChildGPId --NextGeneration.sc_id_parent, NextGeneration.gp_id_parent
			, sc.sc_name, gp.gp_name			
    , CAST(CASE WHEN Parent.Parents = ''
        --THEN('[' + CAST(NextGeneration.sc_id_parent AS VARCHAR(MAX)) + ' ' + CAST(NextGeneration.gp_id_parent AS VARCHAR(MAX)) + ']')
        --ELSE(Parent.Parents + '.' + '[' + CAST(NextGeneration.sc_id_parent AS VARCHAR(MAX)) + ' ' + CAST(NextGeneration.gp_id_parent AS VARCHAR(MAX)) + ']')
		THEN(CAST(Parent.ChildGpId AS VARCHAR(MAX)) + ' ' + CAST(Parent.ChildScId AS VARCHAR(MAX)))
        ELSE(Parent.Parents + ',' + CAST(Parent.ChildGpId AS VARCHAR(MAX)) + ' ' + CAST(Parent.ChildScId AS VARCHAR(MAX)))
    END AS NVARCHAR(MAX))
        FROM space.hierarchy_permissions AS NextGeneration
		inner join space.granular_permissions as gp on gp.gp_id = NextGeneration.gp_id
		inner join space.[securable_classes] as sc on sc.sc_id = NextGeneration.sc_id  
        INNER JOIN Hierarchy AS Parent ON NextGeneration.sc_id_parent = Parent.ChildSCId 
											and NextGeneration.gp_id_parent = Parent.ChildGPId
)
SELECT
		ParentGPId, ParentSCId,
		ChildGPId, ChildSCId,
		Parents
    FROM Hierarchy htc
	left outer join Space.granular_permissions gpParent on gpParent.gp_id = htc.ParentGPId
	left outer join Space.[securable_classes] scParent on scParent.sc_id = htc.ParentSCId
	where ChildGpName = @granularPermissionName and ChildScName = @secureClassName
OPTION(MAXRECURSION 32767)
