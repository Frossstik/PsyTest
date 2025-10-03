import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import Header from "../components/Header";
import { Link, useNavigate } from "react-router-dom";

export default function Home() {
    const [tests, setTests] = useState([]);
    const [search, setSearch] = useState("");
    const [profile, setProfile] = useState(null);
    const { token } = useAuth();

    useEffect(() => {
        fetch(`${import.meta.env.VITE_MAIN_URL}/api/tests`)
            .then((res) => res.json())
            .then((data) => setTests(data));
    }, []);

    useEffect(() => {
        if (token) {
            fetch(`${import.meta.env.VITE_IDENTITY_URL}/profile`, {
                headers: { Authorization: `Bearer ${token}` },
            })
                .then((res) => res.json())
                .then((data) => setProfile(data));
        }
    }, [token]);

    const filteredTests = tests.filter((t) =>
        t.name.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <div className="min-h-screen bg-gray-50">
            <Header profile={profile} />

            <main className="p-6">
                <h2 className="text-2xl font-bold mb-4">Список тестов</h2>
                <input
                    type="text"
                    placeholder="Поиск тестов..."
                    className="px-3 py-1 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] mb-4"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                />
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {filteredTests.map((t) => (
                        <div
                            key={t.id}
                            className="bg-white p-6 rounded-xl shadow hover:shadow-lg transition"
                        >
                            <h3 className="text-lg font-semibold">{t.name}</h3>
                            <p className="text-gray-600">{t.shortDescription}</p>
                            <Link
                                to={`/tests/${t.id}`}
                                className="mt-4 block text-center w-full bg-[color:var(--color-brand)] text-white py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                            >
                                Пройти
                            </Link>
                        </div>
                    ))}
                </div>
            </main>
        </div>
    );
}
