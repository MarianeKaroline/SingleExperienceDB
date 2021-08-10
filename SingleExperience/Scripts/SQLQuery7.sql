CREATE TABLE Category(
	CategoryId INT NOT NULL,
	Description VARCHAR(50) NOT NULL
	PRIMARY KEY (CategoryId));

CREATE TABLE Product( 
	ProductId INT NOT NULL IDENTITY(1,1),
	Name VARCHAR(100) NOT NULL,
	Price FLOAT NOT NULL,
	Detail VARCHAR(500) NOT NULL,
	Amount INT NOT NULL,
	CategoryId INT NOT NULL,
	Ranking INT NOT NULL,
	Available BIT,
	Rating FLOAT
	PRIMARY KEY (ProductId));

CREATE TABLE Enjoyer(
	Cpf VARCHAR(11) NOT NULL,
	Name VARCHAR(50) NOT NULL,
	Phone VARCHAR(15) NOT NULL,
	Email VARCHAR(50) NOT NULL,
	BirthDate DATE NOT NULL,
	Password VARCHAR(50) NOT NULL,
	Employee BIT
	PRIMARY KEY (Cpf));

CREATE TABLE AccessEmployee(
	Cpf VARCHAR(11) NOT NULL,
	AccessInventory BIT,
	AccessRegister BIT,
	PRIMARY KEY (Cpf));
	

CREATE TABLE CreditCard (
	CardId INT NOT NULL IDENTITY(1,1),
	Number VARCHAR(20) NOT NULL,
	Name VARCHAR(20) NOT NULL,
	ShelLife DATE NOT NULL,
	Cvv INT NOT NULL,
	Cpf VARCHAR(11) NOT NULL
	PRIMARY KEY (CardId));


CREATE TABLE Address(
	AddressId INT NOT NULL IDENTITY(1,1),
	Postcode VARCHAR(10) NOT NULL,
	Street VARCHAR(50) NOT NULL,
	Number VARCHAR(5) NOT NULL,
	City VARCHAR(10) NOT NULL,
	State VARCHAR(10) NOT NULL,
	Cpf VARCHAR(11) NOT NULL
	PRIMARY KEY (AddressId));

CREATE TABLE Cart(
	CartId INT NOT NULL IDENTITY(1,1),
	Cpf VARCHAR(11) NOT NULL,
	DateCreated DATE NOT NULL
	PRIMARY KEY (CartId));

	
CREATE TABLE StatusProductCart(
	StatusProductCartId INT NOT NULL,
	Description VARCHAR(50) NOT NULL
	PRIMARY KEY (StatusProductCartId));


CREATE TABLE ItemCart(
	ItemCartId INT NOT NULL IDENTITY(1,1),
	ProductId INT NOT NULL,
	CartId INT NOT NULL,
	Amount INT NOT NULL,
	StatusProductCartId INT NOT NULL
	PRIMARY KEY (ItemCartId));


CREATE TABLE Payment(
	PaymentId INT NOT NULL,
	Description VARCHAR(50) NOT NULL
	PRIMARY KEY (PaymentId));

CREATE TABLE StatusBought(
	StatusBoughtId INT NOT NULL,
	Description VARCHAR(50) NOT NULL
	PRIMARY KEY (StatusBoughtId));


CREATE TABLE Bought(
	BoughtId INT NOT NULL IDENTITY(1,1),
	TotalPrice INT NOT NULL,
	AddressId INT NOT NULL,
	PaymentId INT NOT NULL,
	CodeBought INT NOT NULL,
	Cpf VARCHAR(11) NOT NULL,
	StatusBoughtId INT NOT NULL,
	DateBought DATE NOT NULL
	PRIMARY KEY (BoughtId));		


CREATE TABLE ProductBought(
	ProductBoughtId INT NOT NULL IDENTITY(1,1),
	ProductId INT NOT NULL,
	Amount INT NOT NULL,
	BoughtId INT NOT NULL
	PRIMARY KEY (ProductBoughtId));
	
--- End Create Table


--- Create Alter Table
---CategoryId into Product
ALTER TABLE Product ADD CONSTRAINT FK_Product_Category
FOREIGN KEY (CategoryId) REFERENCES Category (CategoryId);

---Cpf into CreditCard
ALTER TABLE CreditCard ADD CONSTRAINT FK_CreditCard_Enjoyer
FOREIGN KEY (Cpf) REFERENCES Enjoyer (Cpf);

---Cpf into Address
ALTER TABLE Address ADD CONSTRAINT FK_Address_Enjoyer
FOREIGN KEY (Cpf) REFERENCES Enjoyer (Cpf);

---Cpf into Cart
ALTER TABLE Cart ADD CONSTRAINT FK_Cart_Enjoyer
FOREIGN KEY (Cpf) REFERENCES Enjoyer (Cpf);

---CartId into ItemCart
ALTER TABLE ItemCart ADD CONSTRAINT FK_ItemCart_Cart
FOREIGN KEY (CartId) REFERENCES Cart (CartId);

---StatusProductCartId into ItemCart
ALTER TABLE ItemCart ADD CONSTRAINT FK_ItemCart_StatusProductCart
FOREIGN KEY (StatusProductCartId) REFERENCES StatusProductCart (StatusProductCartId);

---ProductId into ItemCart
ALTER TABLE ItemCart ADD CONSTRAINT FK_ItemCart_Product
FOREIGN KEY (ProductId) REFERENCES Product (ProductId);

---Cpf into Bought
ALTER TABLE Bought ADD CONSTRAINT FK_Bought_Enjoyer
FOREIGN KEY (Cpf) REFERENCES Enjoyer (Cpf);

---PaymentId into Bought
ALTER TABLE Bought ADD CONSTRAINT FK_Bought_Payment
FOREIGN KEY (PaymentId) REFERENCES Payment (PaymentId);

---Address into Bought
ALTER TABLE Bought ADD CONSTRAINT FK_Bought_Address
FOREIGN KEY (AddressId) REFERENCES Address (AddressId);

---StatusBoughtId into Bought
ALTER TABLE Bought ADD CONSTRAINT FK_Bought_StatusBought
FOREIGN KEY (StatusBoughtId) REFERENCES StatusBought (StatusBoughtId);

---BoughtId into ProductBought
ALTER TABLE ProductBought ADD CONSTRAINT FK_ProductBought_Bought
FOREIGN KEY (BoughtId) REFERENCES Bought (BoughtId);

---ProductId into ProductBought
ALTER TABLE ProductBought ADD CONSTRAINT FK_ProductBought_Product
FOREIGN KEY (ProductId) REFERENCES Product (ProductId);

---ProductId into ProductBought
ALTER TABLE AccessEmployee ADD CONSTRAINT FK_Access_Enjoyer
FOREIGN KEY (Cpf) REFERENCES Enjoyer (Cpf);

--- Insert Datas Table

INSERT INTO Category (CategoryId, Description)
	VALUES (1, 'Accessories'),
			(2, 'Cellphone'),
			(3, 'Computer'),
			(4, 'Notebook'),
			(5, 'Tablet');

INSERT INTO Enjoyer (Cpf, Name, Phone, Email, BirthDate, Password, Employee)
	VALUES ('10328698954', 'Maria da Silva', '995427605', 'maria@email.com', '19980705', '123456', 'true');

INSERT INTO StatusProductCart (StatusProductCartId, Description)
	VALUES(1, 'Inactive'),
			(2, 'Active'),
			(3, 'Bought');

INSERT INTO Payment (PaymentId, Description)
	VALUES(1, 'Credit Card'),
			(2, 'Bank Slip'),
			(3, 'Pix');

INSERT INTO StatusBought (StatusBoughtId, Description)
	VALUES(1, 'Pending Confirmation'),
			(2, 'Pending Payment'),
			(3, 'Confirmed'),
			(4, 'Canceled');

--- End Insert Datas Table