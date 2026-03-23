INSERT INTO Patients (FirstName, LastName, DateOfBirth)
VALUES ('Jane', 'Smith', '1985-04-12'),
       ('Robert', 'Johnson', '1970-11-30');

INSERT INTO Referrals (PatientId, ReferralSource, ReferralType, ReferralNote)
VALUES (1, 'Hospital', 'Short Stay', 'Patient recently hospitalized with pneumonia.'),
       (1, 'Dr. Adams - GP', 'Cardiology', NULL),
       (2, 'Emergency Department', 'Neurology', 'Recurring headaches, onset 3 weeks ago.');
