CREATE DATABASE BD_Almacen;
GO

USE BD_Almacen;
GO

CREATE TABLE rol (
    rol_id BIGINT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_rol PRIMARY KEY (rol_id)
);

CREATE TABLE usuario (
    usuario_id BIGINT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(80) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    enabled BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_usuario PRIMARY KEY (usuario_id)
);

CREATE TABLE usuario_roles (
    usuario_id BIGINT NOT NULL,
    rol_id BIGINT NOT NULL,
    CONSTRAINT PK_usuario_roles PRIMARY KEY (usuario_id, rol_id),
    CONSTRAINT FK_usuario_roles_usuario FOREIGN KEY (usuario_id) REFERENCES usuario(usuario_id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_usuario_roles_rol FOREIGN KEY (rol_id) REFERENCES rol(rol_id) ON DELETE NO ACTION ON UPDATE CASCADE
);

CREATE TABLE categoria (
    categoria_id SMALLINT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(80) NOT NULL,
    descripcion VARCHAR(255),
    activo BIT NOT NULL DEFAULT 1,
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_categoria PRIMARY KEY (categoria_id),
    CONSTRAINT UQ_categoria_nombre UNIQUE (nombre)
);

CREATE TABLE unidad (
    unidad_id SMALLINT IDENTITY(1,1) NOT NULL,
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(40) NOT NULL,
    factor_base DECIMAL(18,6) NOT NULL DEFAULT 1.000000,
    es_base BIT NOT NULL DEFAULT 1,
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_unidad PRIMARY KEY (unidad_id),
    CONSTRAINT UQ_unidad_codigo UNIQUE (codigo)
);

CREATE TABLE producto (
    producto_id INT IDENTITY(1,1) NOT NULL,
    sku VARCHAR(30) NOT NULL,
    nombre VARCHAR(120) NOT NULL,
    descripcion VARCHAR(500),
    categoria_id SMALLINT NOT NULL,
    codigo_barras VARCHAR(50),
    precio_lista DECIMAL(18,2),
    activo BIT NOT NULL DEFAULT 1,
    creado_en DATETIME NOT NULL DEFAULT GETDATE(),
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_producto PRIMARY KEY (producto_id),
    CONSTRAINT FK_producto_categoria FOREIGN KEY (categoria_id) REFERENCES categoria(categoria_id) ON UPDATE CASCADE,
    CONSTRAINT UQ_producto_sku UNIQUE (sku)
);

-- Índices extra
CREATE INDEX idx_producto_nombre ON producto(nombre);
CREATE INDEX idx_producto_categoria ON producto(categoria_id);

CREATE TABLE producto_unidad (
    producto_id INT NOT NULL,
    unidad_id SMALLINT NOT NULL,
    factor DECIMAL(18,6) NOT NULL DEFAULT 1.000000,
    es_base BIT NOT NULL DEFAULT 0,
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_producto_unidad PRIMARY KEY (producto_id, unidad_id),
    CONSTRAINT FK_producto_unidad_producto FOREIGN KEY (producto_id) REFERENCES producto(producto_id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_producto_unidad_unidad FOREIGN KEY (unidad_id) REFERENCES unidad(unidad_id) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT chk_producto_unidad_factor CHECK (factor > 0)
);

CREATE INDEX idx_producto_unidad_base ON producto_unidad(producto_id, es_base);

CREATE TABLE almacen (
    almacen_id SMALLINT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(80) NOT NULL,
    tipo VARCHAR(20) NOT NULL CHECK (tipo IN ('PRINCIPAL','SECUNDARIO','TRANSITO')),
    direccion VARCHAR(180),
    activo BIT NOT NULL DEFAULT 1,
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_almacen PRIMARY KEY (almacen_id),
    CONSTRAINT UQ_almacen_nombre UNIQUE (nombre)
);

CREATE TABLE inventario (
    almacen_id SMALLINT NOT NULL,
    producto_id INT NOT NULL,
    cantidad DECIMAL(18,6) NOT NULL DEFAULT 0,
    stock_min DECIMAL(18,6) NOT NULL DEFAULT 0,
    stock_max DECIMAL(18,6),
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_inventario PRIMARY KEY (almacen_id, producto_id),
    CONSTRAINT FK_inventario_almacen FOREIGN KEY (almacen_id) REFERENCES almacen(almacen_id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_inventario_producto FOREIGN KEY (producto_id) REFERENCES producto(producto_id) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT chk_inventario_noneg CHECK (cantidad >= 0 AND stock_min >= 0)
);

CREATE INDEX idx_inventario_cantidad ON inventario(cantidad);

CREATE TABLE inventario_movimiento (
    inventario_movimiento_id BIGINT IDENTITY(1,1) NOT NULL,
    fecha_movimiento DATETIME NOT NULL DEFAULT GETDATE(),
    almacen_id SMALLINT NOT NULL,
    producto_id INT NOT NULL,
    tipo_movimiento VARCHAR(20) NOT NULL CHECK (tipo_movimiento IN ('ENTRADA','SALIDA','AJUSTE')),
    cantidad DECIMAL(18,6) NOT NULL,
    costo_unitario DECIMAL(18,6),
    referencia VARCHAR(100),
    usuario VARCHAR(60),
    ultima_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_inv_mov PRIMARY KEY (inventario_movimiento_id),
    CONSTRAINT FK_inv_mov_almacen FOREIGN KEY (almacen_id) REFERENCES almacen(almacen_id) ON UPDATE CASCADE,
    CONSTRAINT FK_inv_mov_producto FOREIGN KEY (producto_id) REFERENCES producto(producto_id) ON UPDATE CASCADE,
    CONSTRAINT chk_inv_mov_cantidad CHECK (cantidad <> 0)
);

CREATE TABLE pedido (
    pedido_id BIGINT IDENTITY(1,1) NOT NULL,
    usuario_id BIGINT NOT NULL,
    fecha DATETIME NOT NULL DEFAULT GETDATE(),
    total DECIMAL(18,2) NOT NULL,
    estado VARCHAR(20) DEFAULT 'PENDIENTE', -- PENDIENTE, PAGADO, ANULADO
    CONSTRAINT PK_pedido PRIMARY KEY (pedido_id),
    CONSTRAINT FK_pedido_usuario FOREIGN KEY (usuario_id) REFERENCES usuario(usuario_id)
);

CREATE TABLE detalle_pedido (
    detalle_id BIGINT IDENTITY(1,1) NOT NULL,
    pedido_id BIGINT NOT NULL,
    producto_id INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_detalle_pedido PRIMARY KEY (detalle_id),
    CONSTRAINT FK_detalle_pedido_pedido FOREIGN KEY (pedido_id) REFERENCES pedido(pedido_id),
    CONSTRAINT FK_detalle_producto FOREIGN KEY (producto_id) REFERENCES producto(producto_id)
);

CREATE INDEX idx_inv_mov_fecha ON inventario_movimiento(fecha_movimiento);
CREATE INDEX idx_inv_mov_ap ON inventario_movimiento(almacen_id, producto_id);


SET IDENTITY_INSERT rol ON;
IF NOT EXISTS (SELECT 1 FROM rol WHERE rol_id = 1) INSERT INTO rol (rol_id, nombre) VALUES (1, 'Admin');
IF NOT EXISTS (SELECT 1 FROM rol WHERE rol_id = 2) INSERT INTO rol (rol_id, nombre) VALUES (2, 'User');
SET IDENTITY_INSERT rol OFF;

-- 2. Insertar Usuarios (si no existen)
-- Contraseña simple: 'password_ejemplo'
SET IDENTITY_INSERT usuario ON;
IF NOT EXISTS (SELECT 1 FROM usuario WHERE usuario_id = 1) 
    INSERT INTO usuario (usuario_id, nombre, email, password_hash, enabled) 
    VALUES (1, 'Gerente General', 'gerente@inka.com', 'password', 1);

IF NOT EXISTS (SELECT 1 FROM usuario WHERE usuario_id = 2) 
    INSERT INTO usuario (usuario_id, nombre, email, password_hash, enabled) 
    VALUES (2, 'Supervisor de Tienda', 'supervisor@inka.com', 'password', 1);
SET IDENTITY_INSERT usuario OFF;

-- 3. Insertar Relaciones (usuario_roles)
IF NOT EXISTS (SELECT 1 FROM usuario_roles WHERE usuario_id = 1 AND rol_id = 1) 
    INSERT INTO usuario_roles (usuario_id, rol_id) VALUES (1, 1);

IF NOT EXISTS (SELECT 1 FROM usuario_roles WHERE usuario_id = 2 AND rol_id = 2) 
    INSERT INTO usuario_roles (usuario_id, rol_id) VALUES (2, 2);

INSERT INTO categoria (nombre, descripcion) VALUES 
('Electrónica', 'Tecnología'),
('Alimentos Secos', 'Abarrotes'),
('Herramientas', 'Ferretería'),
('Limpieza', 'Insumos de aseo'),
('Mobiliario', 'Muebles de oficina');


INSERT INTO unidad (codigo, nombre, es_base) VALUES 
('UND', 'Unidad', 1),
('KG',  'Kilogramo', 1),
('CJA', 'Caja', 0),
('LTR', 'Litro', 1);

INSERT INTO almacen (nombre, tipo, direccion, activo) VALUES 
('Almacén Central Lima', 'PRINCIPAL', 'Av. Argentina 1234', 1),
('Tienda Surco', 'SECUNDARIO', 'Av. Benavides 4500', 1),
('Almacén de Tránsito', 'TRANSITO', 'Camión T-01', 1),
('Almacén de Herramientas', 'SECUNDARIO', 'Jr. Lampa 200', 1);

INSERT INTO producto (sku, nombre, descripcion, categoria_id, precio_lista, activo) VALUES 
('LAP-DELL-X13', 'Laptop Ultrabook XPS 13', 'Core i7 16GB RAM', 1, 4899.99, 1),
('SNK-CHOC-100', 'Barra de Chocolate 100g', 'Cacao 70%', 2, 4.50, 1),
('HMT-TALAD',    'Taladro Percutor 850W', 'Profesional', 3, 250.00, 1),
('MBL-MESA',     'Mesa de Escritorio Minimalista', 'Melamina blanca', 5, 350.00, 1),
('CLE-PISOS',    'Detergente Concentrado 1L', 'Aroma Lavanda', 4, 18.00, 1),
('SNK-ARROZ-5',  'Arroz Superior 5KG', 'Graneado', 2, 22.50, 1),
('LAP-ACC-MOU',  'Mouse Inalámbrico Ergonómico', 'Optico', 1, 85.00, 1),
('HMT-LLAVE',    'Set de Llaves Combinadas (20 pcs)', 'Acero cromo', 3, 150.00, 1),
('CLE-DESINF',   'Desinfectante Aerosol 400ml', 'Mata 99% bacterias', 4, 12.50, 1),
('MBL-ESTAN',    'Estante de Almacenamiento Metálico', '5 niveles', 5, 80.00, 1),
('CEC-AMAZ-099', 'Cecina', 'Carne ahumada de la selva', 2, 10.00, 1),
('ATUN-FLORI-333','ATUNCITO','Lata de atún', 2, 5.00, 1);


INSERT INTO producto_unidad (producto_id, unidad_id, factor, es_base) VALUES 
(1, 1, 1, 1), (2, 1, 1, 1), (3, 1, 1, 1), (4, 1, 1, 1), 
(5, 1, 1, 1), (6, 1, 1, 1), (7, 1, 1, 1), (8, 1, 1, 1), 
(9, 1, 1, 1), (10, 1, 1, 1), (11, 2, 1, 1), (12, 1, 1, 1); 

INSERT INTO inventario (almacen_id, producto_id, cantidad, stock_min, stock_max) VALUES 
(1, 1, 100, 10, 50),     
(1, 2, 100, 100, 1000),  
(1, 3, 100, 15, 50),    
(1, 7, 100, 50, 300),   
(1, 11, 150, 0, 500),    
(1, 12, 110, 0, 500),   
(3, 1, 100, 5, 20),      
(4, 3, 100, 15, 50);   


INSERT INTO inventario_movimiento 
(fecha_movimiento, almacen_id, producto_id, tipo_movimiento, cantidad, referencia, usuario) 
VALUES 
(GETDATE(), 1, 1, 'ENTRADA', 100, 'Saldo Inicial', 'gerente@inka.com'),
(GETDATE(), 1, 2, 'ENTRADA', 100, 'Saldo Inicial', 'gerente@inka.com'),
(GETDATE(), 1, 3, 'ENTRADA', 100, 'Saldo Inicial', 'gerente@inka.com'),
(GETDATE(), 1, 11, 'ENTRADA', 150, 'Saldo Inicial', 'gerente@inka.com');

GO

------------------------------------------------------------
-- 1. INSERTAR PEDIDOS (Cabecera)
------------------------------------------------------------
-- Nota: No insertamos pedido_id porque es IDENTITY (automático)

-- Pedido #1: Una compra grande de tecnología realizada por el Supervisor (ID 2) hace 5 días.
-- Estado: PAGADO
INSERT INTO pedido (usuario_id, fecha, total, estado)
VALUES (2, DATEADD(day, -5, GETDATE()), 10224.98, 'PAGADO');

-- Pedido #2: Una compra de abarrotes realizada por el Gerente (ID 1) hace 2 días.
-- Estado: PENDIENTE
INSERT INTO pedido (usuario_id, fecha, total, estado)
VALUES (1, DATEADD(day, -2, GETDATE()), 90.00, 'PENDIENTE');

-- Pedido #3: Una compra de herramientas realizada por el Supervisor (ID 2) hoy.
-- Estado: ANULADO
INSERT INTO pedido (usuario_id, fecha, total, estado)
VALUES (2, GETDATE(), 250.00, 'ANULADO');

GO

------------------------------------------------------------
-- 2. INSERTAR DETALLES (Productos de cada pedido)
------------------------------------------------------------
-- Asumimos que los IDs generados arriba fueron 1, 2 y 3 respectivamente.
-- Los precios unitarios coinciden con tu tabla 'producto'.

-- Detalles del Pedido #1 (Total: 10,224.98)
-- Contiene: 2 Laptops (ID 1) y 5 Mouses (ID 7)
INSERT INTO detalle_pedido (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
VALUES 
(1, 1, 2, 4899.99, 9799.98), -- 2 * 4899.99
(1, 7, 5, 85.00, 425.00);    -- 5 * 85.00

-- Detalles del Pedido #2 (Total: 90.00)
-- Contiene: 10 Chocolates (ID 2) y 2 Bolsas de Arroz (ID 6)
INSERT INTO detalle_pedido (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
VALUES 
(2, 2, 10, 4.50, 45.00), -- 10 * 4.50
(2, 6, 2, 22.50, 45.00); -- 2 * 22.50

-- Detalles del Pedido #3 (Total: 250.00)
-- Contiene: 1 Taladro (ID 3)
INSERT INTO detalle_pedido (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
VALUES 
(3, 3, 1, 250.00, 250.00); -- 1 * 250.00

GO

CREATE PROCEDURE usp_insertar_producto
    @nombre VARCHAR(120),
    @sku VARCHAR(30),
    @categoria SMALLINT,
    @precio DECIMAL(18,2),
    @stock DECIMAL(18,6)
AS
BEGIN
    -- 1. Insertar el producto en la tabla 'producto'
    INSERT INTO producto (
        nombre, 
        sku, 
        categoria_id, 
        precio_lista, 
        descripcion, 
        codigo_barras,
        activo, 
        creado_en, 
        ultima_actualizacion
    )
    VALUES (
        @nombre, 
        @sku, 
        @categoria, 
        @precio, 
        'Sin descripción', -- Valor por defecto o puedes pasarlo como parámetro
        'SIN-CODIGO',      -- Valor por defecto
        1,                 -- Activo por defecto
        GETDATE(), 
        GETDATE()
    );

    -- 2. Obtener el ID del producto recién creado
    DECLARE @newId INT = SCOPE_IDENTITY();

    -- 3. Inicializar el stock en la tabla 'inventario' 
    -- (Asumiendo que el Almacén Principal es el ID 1)
    INSERT INTO inventario (
        almacen_id, 
        producto_id, 
        cantidad, 
        stock_min, 
        ultima_actualizacion
    )
    VALUES (
        1,       -- ID del Almacén Principal
        @newId, 
        @stock, 
        5,       -- Stock mínimo por defecto
        GETDATE()
    );
END
GO

-- =======================================================
-- 1. SEGURIDAD (Login)
-- =======================================================
GO
CREATE OR ALTER PROCEDURE usp_validar_usuario
    @email VARCHAR(100),
    @password VARCHAR(255)
AS
BEGIN
    -- Retorna el usuario si coincide email y contraseña (y está activo)
    SELECT u.usuario_id, u.nombre, u.email, r.nombre as rol
    FROM usuario u
    INNER JOIN usuario_roles ur ON u.usuario_id = ur.usuario_id
    INNER JOIN rol r ON ur.rol_id = r.rol_id
    WHERE u.email = @email 
      AND u.password_hash = @password 
      AND u.enabled = 1;
END
GO

-- =======================================================
-- 2. MANTENIMIENTO DE PRODUCTOS (CRUD Completo)
-- =======================================================
GO
-- A. LISTAR CON PAGINACIÓN Y FILTRO (Cumple criterio de Reportes)
CREATE OR ALTER PROCEDURE usp_listar_productos
    @nombre VARCHAR(100) = NULL,
    @pagina INT = 1,
    @registros_por_pagina INT = 10
AS
BEGIN
    DECLARE @offset INT = (@pagina - 1) * @registros_por_pagina;

    SELECT 
        p.producto_id, 
        p.sku, 
        p.nombre, 
        c.nombre as categoria, 
        p.precio_lista, 
        p.activo,
        COUNT(*) OVER() as total_registros -- Para saber cuántas páginas hay
    FROM producto p
    INNER JOIN categoria c ON p.categoria_id = c.categoria_id
    WHERE (@nombre IS NULL OR p.nombre LIKE '%' + @nombre + '%')
    ORDER BY p.producto_id
    OFFSET @offset ROWS FETCH NEXT @registros_por_pagina ROWS ONLY;
END
GO

-- B. OBTENER UNO (Para editar)
CREATE OR ALTER PROCEDURE usp_obtener_producto
    @id INT
AS
BEGIN
    SELECT * FROM producto WHERE producto_id = @id;
END
GO

-- C. INSERTAR (Ya lo tenías, lo incluyo para completar el set)
CREATE OR ALTER PROCEDURE usp_insertar_producto
    @nombre VARCHAR(120),
    @sku VARCHAR(30),
    @categoria SMALLINT,
    @precio DECIMAL(18,2),
    @stock_inicial DECIMAL(18,6)
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Insertar Producto
        INSERT INTO producto (nombre, sku, categoria_id, precio_lista, descripcion, activo, creado_en, ultima_actualizacion)
        VALUES (@nombre, @sku, @categoria, @precio, 'Sin descripción', 1, GETDATE(), GETDATE());

        DECLARE @newId INT = SCOPE_IDENTITY();

        -- 2. Inicializar Inventario (Almacén Principal ID 1)
        INSERT INTO inventario (almacen_id, producto_id, cantidad, stock_min, ultima_actualizacion)
        VALUES (1, @newId, @stock_inicial, 5, GETDATE());

        -- 3. Registrar Movimiento Inicial
        INSERT INTO inventario_movimiento (fecha_movimiento, almacen_id, producto_id, tipo_movimiento, cantidad, referencia, usuario, ultima_actualizacion)
        VALUES (GETDATE(), 1, @newId, 'ENTRADA', @stock_inicial, 'Inventario Inicial', 'SYSTEM', GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- D. ACTUALIZAR
CREATE OR ALTER PROCEDURE usp_actualizar_producto
    @id INT,
    @nombre VARCHAR(120),
    @categoria SMALLINT,
    @precio DECIMAL(18,2),
    @activo BIT
AS
BEGIN
    UPDATE producto
    SET nombre = @nombre,
        categoria_id = @categoria,
        precio_lista = @precio,
        activo = @activo,
        ultima_actualizacion = GETDATE()
    WHERE producto_id = @id;
END
GO

-- E. ELIMINAR (Lógico, no físico para mantener integridad)
CREATE OR ALTER PROCEDURE usp_eliminar_producto
    @id INT
AS
BEGIN
    UPDATE producto SET activo = 0 WHERE producto_id = @id;
END
GO

-- =======================================================
-- 3. REPORTES (Para cumplir el criterio VI de la rúbrica)
-- =======================================================
GO
-- A. REPORTE DE VENTAS POR RANGO DE FECHAS
CREATE OR ALTER PROCEDURE usp_reporte_ventas
    @fecha_inicio DATETIME,
    @fecha_fin DATETIME
AS
BEGIN
    SELECT 
        p.pedido_id,
        u.nombre as cliente,
        p.fecha,
        p.total,
        p.estado,
        (SELECT COUNT(*) FROM detalle_pedido WHERE pedido_id = p.pedido_id) as items
    FROM pedido p
    INNER JOIN usuario u ON p.usuario_id = u.usuario_id
    WHERE p.fecha BETWEEN @fecha_inicio AND @fecha_fin
    ORDER BY p.fecha DESC;
END
GO

-- B. HISTORIAL DE PEDIDOS DE UN USUARIO (Para la vista "Mis Compras")
CREATE OR ALTER PROCEDURE usp_mis_pedidos
    @usuario_id BIGINT
AS
BEGIN
    SELECT 
        pedido_id,
        fecha,
        total,
        estado
    FROM pedido
    WHERE usuario_id = @usuario_id
    ORDER BY fecha DESC;
END
GO

-- =======================================================
-- 4. UTILITARIOS (Para combos en las vistas)
-- =======================================================
GO
CREATE OR ALTER PROCEDURE usp_listar_categorias_combo
AS
BEGIN
    SELECT categoria_id, nombre FROM categoria WHERE activo = 1 ORDER BY nombre;
END
GO

CREATE OR ALTER PROCEDURE usp_insertar_categoria
    @nombre VARCHAR(80),
    @descripcion VARCHAR(255)
AS
BEGIN
    INSERT INTO categoria (nombre, descripcion, activo, ultima_actualizacion)
    VALUES (@nombre, @descripcion, 1, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE usp_actualizar_categoria
    @id SMALLINT,
    @nombre VARCHAR(80),
    @descripcion VARCHAR(255)
AS
BEGIN
    UPDATE categoria
    SET nombre = @nombre,
        descripcion = @descripcion,
        ultima_actualizacion = GETDATE()
    WHERE categoria_id = @id;
END
GO

CREATE OR ALTER PROCEDURE usp_eliminar_categoria
    @id SMALLINT
AS
BEGIN
    UPDATE categoria SET activo = 0, ultima_actualizacion = GETDATE() WHERE categoria_id = @id;
END
GO

-- =============================================
-- PROCEDIMIENTOS PARA USUARIOS
-- =============================================
GO
-- Para crear usuario (registrar)
CREATE OR ALTER PROCEDURE usp_registrar_usuario
    @nombre VARCHAR(80),
    @email VARCHAR(100),
    @password VARCHAR(255),
    @rol_id BIGINT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Insertar Usuario
        INSERT INTO usuario (nombre, email, password_hash, enabled)
        VALUES (@nombre, @email, @password, 1);
        
        DECLARE @newUserId BIGINT = SCOPE_IDENTITY();

        -- 2. Asignar Rol
        INSERT INTO usuario_roles (usuario_id, rol_id)
        VALUES (@newUserId, @rol_id);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Para actualizar usuario
CREATE OR ALTER PROCEDURE usp_actualizar_usuario
    @id BIGINT,
    @nombre VARCHAR(80),
    @email VARCHAR(100),
    @enabled BIT
AS
BEGIN
    UPDATE usuario
    SET nombre = @nombre,
        email = @email,
        enabled = @enabled
    WHERE usuario_id = @id;
END
GO

-- 1. SP para ACTUALIZAR
CREATE OR ALTER PROCEDURE usp_actualizar_producto
    @id INT,
    @nombre VARCHAR(120),
    @categoria SMALLINT,
    @precio DECIMAL(18,2),
    @descripcion VARCHAR(500)
AS
BEGIN
    UPDATE producto
    SET nombre = @nombre,
        categoria_id = @categoria,
        precio_lista = @precio,
        descripcion = @descripcion,
        ultima_actualizacion = GETDATE()
    WHERE producto_id = @id;
END
GO

-- 2. SP para ELIMINAR (Eliminación Lógica)
CREATE OR ALTER PROCEDURE usp_eliminar_producto
    @id INT
AS
BEGIN
    UPDATE producto 
    SET activo = 0, 
        ultima_actualizacion = GETDATE() 
    WHERE producto_id = @id;
END
GO
