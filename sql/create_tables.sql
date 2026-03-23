CREATE TABLE Patients (
    PatientId   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    FirstName   VARCHAR(100)  NOT NULL,
    LastName    VARCHAR(100)  NOT NULL,
    DateOfBirth DATE          NOT NULL,
    CreatedDate DATETIME      NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Referrals (
    ReferralId     INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    PatientId      INT            NOT NULL,
    ReferralSource VARCHAR(200)   NOT NULL,
    ReferralType   VARCHAR(100)   NOT NULL,
    ReferralNote   NVARCHAR(MAX)  NULL,
    CreatedDate    DATETIME       NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Referrals_Patients
        FOREIGN KEY (PatientId) REFERENCES Patients(PatientId)
        ON DELETE CASCADE
);

CREATE INDEX IX_Referrals_PatientId ON Referrals (PatientId);
CREATE INDEX IX_Referrals_PatientId_CreatedDate ON Referrals (PatientId, CreatedDate DESC);
