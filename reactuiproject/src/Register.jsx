import React, { useState } from "react";

const styles = {
    container: {
        width: "100%",
        height: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "#f2f2f2",
    },
    form: {
        width: "350px",
        padding: "20px",
        background: "#fff",
        borderRadius: "10px",
        boxShadow: "0px 0px 10px rgba(0,0,0,0.1)",
    },
    input: {
        width: "100%",
        padding: "10px",
        marginBottom: "10px",
        borderRadius: "5px",
        border: "1px solid #ccc",
    },
    button: {
        width: "100%",
        padding: "10px",
        background: "#4caf50",
        color: "#fff",
        border: "none",
        borderRadius: "5px",
        fontSize: "16px",
        cursor: "pointer",
    },
    error: {
        color: "red",
        marginTop: "-7px",
        marginBottom: "8px",
        fontSize: "13px",
    },
};

export default function Register() {
    const [form, setForm] = useState({
        name: "",
        email: "",
        password: "",
        confirmPassword: "",
    });

    const [errors, setErrors] = useState({});

    // Handle input change
    const handleChange = (e) => {
        setForm({
            ...form,
            [e.target.name]: e.target.value,
        });
    };

    // Validation function
    const validate = () => {
        let error = {};

        if (!form.name.trim()) {
            error.name = "Name is required";
        }

        if (!form.email) {
            error.email = "Email is required";
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
            error.email = "Enter a valid email";
        }

        if (!form.password) {
            error.password = "Password is required";
        } else if (form.password.length < 6) {
            error.password = "Password must be at least 6 characters";
        }

        if (!form.confirmPassword) {
            error.confirmPassword = "Confirm Password is required";
        } else if (form.confirmPassword !== form.password) {
            error.confirmPassword = "Passwords do not match";
        }

        setErrors(error);
        return Object.keys(error).length === 0;
    };

    // Submit
    const handleSubmit = (e) => {
        e.preventDefault();

        if (validate()) {
            alert("Registration Successful!");
            console.log("User Data:", form);
        }
    };

    return (
        <div style={styles.container}>
            <form onSubmit={handleSubmit} style={styles.form}>
                <h2 style={{ textAlign: "center" }}>Register</h2>

                {/* Name */}
                <label>Name</label>
                <input
                    type="text"
                    name="name"
                    style={styles.input}
                    value={form.name}
                    onChange={handleChange}
                />
                {errors.name && <p style={styles.error}>{errors.name}</p>}

                {/* Email */}
                <label>Email</label>
                <input
                    type="email"
                    name="email"
                    style={styles.input}
                    value={form.email}
                    onChange={handleChange}
                />
                {errors.email && <p style={styles.error}>{errors.email}</p>}

                {/* Password */}
                <label>Password</label>
                <input
                    type="password"
                    name="password"
                    style={styles.input}
                    value={form.password}
                    onChange={handleChange}
                />
                {errors.password && <p style={styles.error}>{errors.password}</p>}

                {/* Confirm Password */}
                <label>Confirm Password</label>
                <input
                    type="password"
                    name="confirmPassword"
                    style={styles.input}
                    value={form.confirmPassword}
                    onChange={handleChange}
                />
                {errors.confirmPassword && (
                    <p style={styles.error}>{errors.confirmPassword}</p>
                )}

                <button type="submit" style={styles.button}>
                    Register
                </button>
            </form>
        </div>
    );
}
