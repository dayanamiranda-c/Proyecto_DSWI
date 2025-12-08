-- Asegúrate de estar en la base de datos correcta (ej. erp_productos)
USE [BD_Almacen]; 

-- 1. Insertar Roles (si no existen)
-- Se requiere activar IDENTITY_INSERT si las columnas son identity
SET IDENTITY_INSERT rol ON;
IF NOT EXISTS (SELECT 1 FROM rol WHERE rol_id = 1) INSERT INTO rol (rol_id, nombre) VALUES (1, 'Admin');
IF NOT EXISTS (SELECT 1 FROM rol WHERE rol_id = 2) INSERT INTO rol (rol_id, nombre) VALUES (2, 'User');
SET IDENTITY_INSERT rol OFF;

-- 2. Insertar Usuarios (si no existen)
-- Contraseña simple: 'password_ejemplo'
SET IDENTITY_INSERT usuario ON;
IF NOT EXISTS (SELECT 1 FROM usuario WHERE usuario_id = 1) 
    INSERT INTO usuario (usuario_id, nombre, email, password_hash, enabled) 
    VALUES (1, 'Gerente General', 'gerente@erp.com', 'password', 1);

IF NOT EXISTS (SELECT 1 FROM usuario WHERE usuario_id = 2) 
    INSERT INTO usuario (usuario_id, nombre, email, password_hash, enabled) 
    VALUES (2, 'Supervisor de Tienda', 'supervisor@erp.com', 'password', 1);
SET IDENTITY_INSERT usuario OFF;

-- 3. Insertar Relaciones (usuario_roles)
IF NOT EXISTS (SELECT 1 FROM usuario_roles WHERE usuario_id = 1 AND rol_id = 1) 
    INSERT INTO usuario_roles (usuario_id, rol_id) VALUES (1, 1);

IF NOT EXISTS (SELECT 1 FROM usuario_roles WHERE usuario_id = 2 AND rol_id = 2) 
    INSERT INTO usuario_roles (usuario_id, rol_id) VALUES (2, 2);

SELECT '¡Verificación y Seeding finalizado!' as Resultado;

Select * from usuario;
Select * from rol;
Select * from usuario_roles;
SELECT
    usuario_id,
    email,
    password_hash,
    LEN(email) as Longitud_Email
FROM usuario
WHERE email = 'gerente@erp.com';