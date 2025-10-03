import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Register() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirm, setConfirm] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("");
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleRegister = async (e) => {
        e.preventDefault();

        if (password !== confirm) {
            alert("Пароли не совпадают");
            return;
        }

        // 1. Регистрируем
        const res = await fetch(`${import.meta.env.VITE_IDENTITY_URL}/register`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                userName: email, // пока используем email как логин
                email,
                password,
                firstName,
                lastName,
                phoneNumber
            }),
        });

        if (res.ok) {
            // 2. Автоматический вход
            const loginRes = await fetch(`${import.meta.env.VITE_IDENTITY_URL}/login`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password }),
            });

            if (loginRes.ok) {
                const data = await loginRes.json();
                login(data.token);
                navigate("/");
            } else {
                alert("Регистрация прошла, но вход не удался");
                navigate("/login");
            }
        } else {
            alert("Ошибка регистрации");
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[color:var(--color-brand-light)] to-[color:var(--color-brand-dark)]">
            <div className="w-full max-w-md bg-white shadow-xl rounded-2xl p-8">
                <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
                    Регистрация
                </h2>
                <form onSubmit={handleRegister} className="space-y-4">
                    <input
                        type="text"
                        placeholder="Имя"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                        value={firstName}
                        onChange={(e) => setFirstName(e.target.value)}
                    />
                    <input
                        type="text"
                        placeholder="Фамилия"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                        value={lastName}
                        onChange={(e) => setLastName(e.target.value)}
                    />
                    <input
                        type="text"
                        placeholder="Телефон"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                        value={phoneNumber}
                        onChange={(e) => setPhoneNumber(e.target.value)}
                    />
                    <input
                        type="email"
                        placeholder="Email"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                    <input
                        type="password"
                        placeholder="Пароль"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                    <input
                        type="password"
                        placeholder="Подтвердите пароль"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                        value={confirm}
                        onChange={(e) => setConfirm(e.target.value)}
                    />
                    <button
                        type="submit"
                        className="w-full bg-[color:var(--color-brand)] text-white font-semibold py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                    >
                        Зарегистрироваться
                    </button>
                </form>
            </div>
        </div>
    );
}
