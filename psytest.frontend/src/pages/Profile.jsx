import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import Header from "../components/Header";
import { Link } from "react-router-dom";

export default function Profile() {
    const [profile, setProfile] = useState(null);
    const { token, logout } = useAuth();

    useEffect(() => {
        if (token) {
            fetch(`${import.meta.env.VITE_IDENTITY_URL}/profile`, {
                headers: { Authorization: `Bearer ${token}` },
            })
                .then((res) => {
                    if (!res.ok) throw new Error("Unauthorized");
                    return res.json();
                })
                .then((data) => setProfile(data))
                .catch(() => logout());
        }
    }, [token, logout]);

    if (!profile) return <div className="p-6">Загрузка...</div>;

    return (
        <div className="min-h-screen bg-gray-50">
            <Header profile={profile} />

            <main className="max-w-xl mx-auto p-6 mt-8 bg-white shadow rounded-xl">
                <h2 className="text-2xl font-bold mb-4 text-gray-800">Профиль</h2>
                <p><b>Email:</b> {profile.email}</p>
                <p><b>Имя:</b> {profile.firstName}</p>
                <p><b>Фамилия:</b> {profile.lastName}</p>
                <p><b>Телефон:</b> {profile.phoneNumber || "не указан"}</p>
                <p><b>Email подтверждён:</b> {profile.emailConfirmed ? "Да" : "Нет"}</p>
                <p><b>Телефон подтверждён:</b> {profile.phoneNumberConfirmed ? "Да" : "Нет"}</p>

                <Link
                    to="/profile/edit"
                    className="mt-4 inline-block bg-[color:var(--color-brand)] text-white px-4 py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Изменить
                </Link>
            </main>
        </div>
    );
}
