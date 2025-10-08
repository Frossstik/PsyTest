import React, { useEffect, useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Header from "../../components/Header";

export default function StaiTest() {
    const { id } = useParams(); // testId (не sessionId!)
    const [searchParams] = useSearchParams();
    const sessionId = searchParams.get("sessionId");
    const { token } = useAuth();
    const navigate = useNavigate();

    const [age, setAge] = useState("");
    const [answers, setAnswers] = useState(Array(40).fill(null));

    const QUESTIONS = [
        "Я спокоен",
        "Мне ничто не угрожает",
        "Я нахожусь в напряжении",
        "Я внутренне скован",
        "Я чувствую себя свободно",
        "Я расстроен",
        "Меня волнуют возможные неудачи",
        "Я ощущаю душевный покой",
        "Я встревожен",
        "Я испытываю чувство внутреннего удовлетворения",
        "Я уверен в себе",
        "Я нервничаю",
        "Я не нахожу себе места",
        "Я взвинчен",
        "Я не чувствую скованности, напряжения",
        "Я доволен",
        "Я озабочен",
        "Я слишком возбужден и мне не по себе",
        "Мне радостно",
        "Мне приятно",
        "У меня бывает приподнятое настроение",
        "Я бываю раздражительным",
        "Я легко расстраиваюсь",
        "Я хотел бы быть таким же удачливым, как и другие",
        "Я сильно переживаю неприятности и долго не могу о них забыть",
        "Я чувствую прилив сил и желание работать",
        "Я спокоен, хладнокровен и собран",
        "Меня тревожат возможные трудности",
        "Я слишком переживаю из-за пустяков",
        "Я бываю вполне счастлив",
        "Я все принимаю близко к сердцу",
        "Мне не хватает уверенности в себе",
        "Я чувствую себя беззащитным",
        "Я стараюсь избегать критических ситуаций и трудностей",
        "У меня бывает хандра",
        "Я бываю доволен",
        "Всякие пустяки отвлекают и волнуют меня",
        "Бывает, что я чувствую себя неудачником",
        "Я уравновешенный человек",
        "Меня охватывает беспокойство, когда я думаю о своих делах и заботах"
    ];


    // Загружаем сохранённые ответы
    useEffect(() => {
        const saved = JSON.parse(localStorage.getItem(`stai-${sessionId}`));
        if (saved) {
            setAnswers(saved.answers || Array(40).fill(null));
            setAge(saved.age || "");
        }
    }, [sessionId]);

    // Сохраняем прогресс
    useEffect(() => {
        localStorage.setItem(`stai-${sessionId}`, JSON.stringify({ answers, age }));
    }, [answers, age, sessionId]);

    const handleAnswer = (qIndex, value) => {
        const updated = [...answers];
        updated[qIndex] = value;
        setAnswers(updated);
    };

    const handleSubmit = async () => {
        if (!age || isNaN(age) || age < 13 || age > 99) {
            alert("Введите корректный возраст (от 13 до 99 лет)");
            return;
        }
        if (answers.includes(null)) {
            alert("Пожалуйста, ответьте на все 40 вопросов");
            return;
        }

        const dto = { age: parseInt(age), answers };

        const res = await fetch(
            `${import.meta.env.VITE_MAIN_URL}/api/Tests/${sessionId}/stai`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify(dto),
            }
        );

        if (res.ok) {
            localStorage.removeItem(`stai-${sessionId}`);
            navigate(`/results/${sessionId}`);
        } else {
            const err = await res.text();
            console.error("❌ Ошибка сохранения:", err);
            alert("Ошибка при сохранении результатов");
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <main className="max-w-4xl mx-auto p-6">
                <h2 className="text-2xl font-bold mb-6 text-gray-800">
                    Тест Спилбергера–Ханина (STAI)
                </h2>
                <h3 className="text-2xl font-bold mb-6 text-gray-800">
                    1 - Никогда,
                    2 - Почти никогда,
                    3 - Часто,
                    4 - Почти всегда.
                </h3>

                {/* Возраст */}
                <div className="mb-6">
                    <label className="block mb-2 font-medium text-gray-700">
                        Ваш возраст:
                    </label>
                    <input
                        type="number"
                        value={age}
                        onChange={(e) => setAge(e.target.value)}
                        className="border rounded-lg px-3 py-2 w-32 text-center"
                        placeholder="Возраст"
                    />
                </div>

                {/* Вопросы */}
                <div className="space-y-6">
                    {QUESTIONS.map((q, i) => (
                        <div
                            key={i}
                            className="bg-white shadow p-4 rounded-lg border border-gray-200"
                        >
                            <p className="font-medium mb-3">{i + 1}. {q}</p>
                            <div className="flex gap-2 flex-wrap">
                                {[1, 2, 3, 4].map((val) => (
                                    <button
                                        key={val}
                                        onClick={() => handleAnswer(i, val)}
                                        className={`px-3 py-1 rounded-lg border ${answers[i] === val
                                            ? "bg-[color:var(--color-brand)] text-white"
                                            : "bg-gray-100 hover:bg-gray-200"
                                            }`}
                                    >
                                        {val}
                                    </button>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>

                {/* Кнопка завершения */}
                <button
                    onClick={handleSubmit}
                    className="mt-8 px-6 py-2 bg-[color:var(--color-brand)] text-white rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Завершить тест
                </button>
            </main>
        </div>
    );
}
