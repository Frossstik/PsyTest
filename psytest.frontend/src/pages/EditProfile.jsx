import React, { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import Header from "../components/Header";

export default function EditProfile() {
    const { token, logout } = useAuth();
    const navigate = useNavigate();
    const [profile, setProfile] = useState({
        firstName: "",
        lastName: "",
        phoneNumber: "",
        email: ""
    });

    useEffect(() => {
        if (token) {
            fetch(`${import.meta.env.VITE_IDENTITY_URL}/profile`, {
                headers: { Authorization: `Bearer ${token}` },
            })
                .then((res) => {
                    if (!res.ok) throw new Error("Unauthorized");
                    return res.json();
                })
                .then((data) =>
                    setProfile({
                        firstName: data.firstName || "",
                        lastName: data.lastName || "",
                        phoneNumber: data.phoneNumber || "",
                        email: data.email || ""
                    })
                )
                .catch(() => logout());
        }
    }, [token, logout]);

    const handleChange = (e) => {
        setProfile({ ...profile, [e.target.name]: e.target.value });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        fetch(`${import.meta.env.VITE_IDENTITY_URL}/profile`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(profile),
        })
            .then((res) => {
                if (!res.ok) throw new Error("Ошибка обновления профиля");
                return res.text();
            })
            .then(() => {
                alert("Профиль обновлен");
                navigate("/profile");
            })
            .catch((err) => alert(err.message));
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header profile={profile} />

            <main className="max-w-xl mx-auto p-6 mt-8 bg-white shadow rounded-xl">
                <h2 className="text-2xl font-bold mb-4 text-gray-800">Редактирование профиля</h2>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block mb-1 font-medium">Имя</label>
                        <input
                            type="text"
                            name="firstName"
                            value={profile.firstName}
                            onChange={handleChange}
                            className="w-full border px-3 py-2 rounded-lg"
                        />
                    </div>
                    <div>
                        <label className="block mb-1 font-medium">Фамилия</label>
                        <input
                            type="text"
                            name="lastName"
                            value={profile.lastName}
                            onChange={handleChange}
                            className="w-full border px-3 py-2 rounded-lg"
                        />
                    </div>
                    <div>
                        <label className="block mb-1 font-medium">Телефон</label>
                        <input
                            type="text"
                            name="phoneNumber"
                            value={profile.phoneNumber}
                            onChange={handleChange}
                            className="w-full border px-3 py-2 rounded-lg"
                        />
                    </div>
                    <div>
                        <label className="block mb-1 font-medium">Email</label>
                        <input
                            type="email"
                            name="email"
                            value={profile.email}
                            onChange={handleChange}
                            className="w-full border px-3 py-2 rounded-lg"
                        />
                    </div>
                    <button
                        type="submit"
                        className="bg-[color:var(--color-brand)] text-white px-4 py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                    >
                        Сохранить изменения
                    </button>
                </form>
            </main>
        </div>
    );
}
