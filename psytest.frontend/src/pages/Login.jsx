import React, { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { Link } from "react-router-dom";
import { useNavigate } from "react-router-dom";

export default function Login() {
    const { login } = useAuth();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();

        const res = await fetch(`${import.meta.env.VITE_IDENTITY_URL}/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password }),
        });

        if (res.ok) {
            const data = await res.json();
            login(data.token);
            navigate("/");
        } else {
            alert("Ошибка входа");
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[color:var(--color-brand-light)] to-[color:var(--color-brand-dark)]">
            <div className="w-full max-w-md bg-white shadow-xl rounded-2xl p-8">
                <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
                    Вход в систему
                </h2>
                <form onSubmit={handleSubmit} className="space-y-4">
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
                    <button
                        type="submit"
                        className="w-full bg-[color:var(--color-brand)] text-white font-semibold py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                    >
                        Войти
                    </button>
                </form>
                <div className="mt-6 text-center">
                    <p className="text-gray-600">Нет аккаунта?</p>
                    <Link
                        to="/register"
                        className="mt-2 inline-block bg-[color:var(--color-brand)] text-white py-2 px-4 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                    >
                        Зарегистрироваться
                    </Link>
                </div>
            </div>
        </div>
    );
}
