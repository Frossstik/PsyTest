import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import Header from "../components/Header";

export default function Test() {
    const { id } = useParams(); // testId
    const navigate = useNavigate();
    const { token } = useAuth();
    const [test, setTest] = useState(null);

    // Загружаем данные теста
    useEffect(() => {
        fetch(`${import.meta.env.VITE_MAIN_URL}/api/tests/${id}`)
            .then((res) => res.json())
            .then((data) => setTest(data));
    }, [id]);

    if (!test) return <div className="p-6">Загрузка...</div>;

    const startTest = async () => {
        if (!token) {
            alert("Необходимо войти, чтобы пройти тест");
            navigate("/login");
            return;
        }

        const res = await fetch(
            `${import.meta.env.VITE_MAIN_URL}/api/Sessions/${id}/sessions`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
            }
        );

        if (res.ok) {
            const session = await res.json();
            if (test.name.toLowerCase().includes("pbq")) {
                navigate(`/tests/${id}/pbq?sessionId=${session.sessionId}`);
            } else if (test.name.toLowerCase().includes("люшер")) {
                navigate(`/tests/${id}/luscher?sessionId=${session.sessionId}`);
            } else if (test.name.toLowerCase().includes("шмишека")) {
                navigate(`/tests/${id}/schmieschek?sessionId=${session.sessionId}`);
            } else if (test.name.toLowerCase().includes("stai")) {
                navigate(`/tests/${id}/stai?sessionId=${session.sessionId}`);
            } else {
                console.log(test.name);
                alert("Неизвестный тип теста");
            }
        } else {
            alert("Ошибка при создании сессии");
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <main className="max-w-3xl mx-auto p-6 bg-white shadow rounded-xl mt-6">
                <h2 className="text-2xl font-bold mb-4">{test.name}</h2>
                <p className="text-gray-700 mb-6">
                    {test.fullDescription || test.shortDescription}
                </p>
                <button
                    onClick={startTest}
                    className="bg-[color:var(--color-brand)] text-white px-6 py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Пройти тест
                </button>
            </main>
        </div>
    );
}
