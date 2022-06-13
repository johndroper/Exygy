

CREATE TABLE Properties
(
	property_id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	property_name NVARCHAR(255) NOT NULL,
	property_picture_url VARCHAR(512) NOT NULL
)
GO

CREATE TABLE Units
(
	unit_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	property_id UNIQUEIDENTIFIER NOT NULL,
	unit_type VARCHAR(32) NOT NULL,
	unit_min_occupancy INT NOT NULL,
	unit_max_occupancy INT NOT NULL,
	unit_sqft INT NOT NULL
)
GO


CREATE TABLE Unit_Amenities
(
	unit_id INT NOT NULL,
	unit_amenity NVARCHAR(255) NOT NULL,
	PRIMARY KEY(unit_id, unit_amenity)
)
GO

--Data Check
SELECT * FROM Properties;
SELECT * FROM Unit_Amenities;

--Cleanup
TRUNCATE TABLE Properties;
TRUNCATE TABLE Units;
TRUNCATE TABLE Unit_Amenities;
GO

SELECT
    p.property_id,
    p.property_name,
    p.property_picture_url,
	t1.avg_unit_sqft,
	t1.min_min_occupancy,
	t1.max_max_occupancy
FROM
    Properties p
	JOIN (
		SELECT
			p1.property_id,
			AVG(u1.unit_sqft) avg_unit_sqft,
			MIN(u1.unit_min_occupancy) min_min_occupancy,
			MAX(u1.unit_max_occupancy) max_max_occupancy
		FROM
			properties p1
		JOIN units u1
		ON
			p1.property_id = u1.property_id
		GROUP BY
			p1.property_id) t1
	ON p.property_id = t1.property_id
	JOIN (
		SELECT DISTINCT
			u2.property_id
		FROM
			Units u2
			JOIN Unit_Amenities ua
			ON u2.unit_id = ua.unit_id
		WHERE
			ua.unit_amenity = 'pet friendly'
	) t2
	ON p.property_id = t2.property_id
ORDER BY
    p.property_name
OFFSET 0 ROWS
FETCH NEXT 10 ROWS ONLY

SELECT
	*
FROM
	Units u
WHERE
	u.property_id = '1EEB84CA-4575-4D6C-8E11-1BEB8B0FA9DC'



SELECT
	u.unit_type,
	AVG(u.unit_sqft) avg_unit_sqft,
	MIN(u.unit_min_occupancy) min_min_occupancy,
	MAX(u.unit_max_occupancy) max_max_occupancy
FROM
	Units u
WHERE
	u.property_id = '1EEB84CA-4575-4D6C-8E11-1BEB8B0FA9DC'
GROUP BY
	u.unit_type
ORDER BY
	AVG(u.unit_sqft)

	select MAX(unit_max_occupancy) from units

	SELECT DISTINCT ua.unit_amenity
	FROM Unit_Amenities ua

	SELECT 
        u2.property_id,
		COUNT(distinct ua.unit_amenity) 
        FROM
        Units u2
        JOIN Unit_Amenities ua
        ON u2.unit_id = ua.unit_id
	GROUP BY
		u2.property_id

                   
