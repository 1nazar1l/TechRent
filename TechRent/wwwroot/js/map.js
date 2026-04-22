let map;
let deliveryMark;
let officePlacemarks = [];
let offices = [];
let selectedDeliveryPoint = null;

// Тариф доставки: рублей за км
const DELIVERY_RATE_PER_KM = 50;

function initMap() {
    if (typeof ymaps === 'undefined') {
        setTimeout(initMap, 500);
        return;
    }

    ymaps.ready(async function () {
        const defaultCenter = [55.751574, 37.573856];

        map = new ymaps.Map("map", {
            center: defaultCenter,
            zoom: 11,
            controls: ['zoomControl', 'fullscreenControl']
        });

        addSearchControl();
        await loadOffices();

        map.events.add('click', function (e) {
            const coords = e.get('coords');
            setDeliveryPoint(coords[0], coords[1]);
        });
    });
}

async function loadOffices() {
    try {
        const response = await fetch('/Admin/GetOffices');
        if (!response.ok) return;
        offices = await response.json();

        offices.forEach(function (office) {
            const mark = new ymaps.Placemark(
                [office.latitude, office.longitude],
                {
                    balloonContent: `<strong>${office.name}</strong>${office.address ? '<br>' + office.address : ''}`
                },
                {
                    preset: 'islands#blueHomeCircleIcon'
                }
            );
            map.geoObjects.add(mark);
            officePlacemarks.push(mark);
        });

        // Центрируем карту на первом офисе
        if (offices.length > 0) {
            map.setCenter([offices[0].latitude, offices[0].longitude], 11);
        }
    } catch (e) {
        console.error('Ошибка загрузки офисов:', e);
    }
}

function setDeliveryPoint(lat, lng) {
    if (deliveryMark) {
        map.geoObjects.remove(deliveryMark);
    }

    deliveryMark = new ymaps.Placemark([lat, lng], {
        balloonContent: 'Точка доставки'
    }, {
        preset: 'islands#redDotIcon',
        draggable: true
    });

    deliveryMark.events.add('dragend', function () {
        const coords = deliveryMark.geometry.getCoordinates();
        updateDeliveryInfo(coords[0], coords[1]);
    });

    map.geoObjects.add(deliveryMark);
    updateDeliveryInfo(lat, lng);
}

function updateDeliveryInfo(lat, lng) {
    selectedDeliveryPoint = { lat, lng };
    document.getElementById('deliveryLat').value = lat;
    document.getElementById('deliveryLng').value = lng;

    // Получаем адрес
    getAddressByCoords(lat, lng);

    // Считаем расстояние до ближайшего офиса и цену
    if (offices.length > 0) {
        const nearest = findNearestOffice(lat, lng);
        const distKm = nearest.distanceKm;
        const deliveryCost = Math.round(distKm * DELIVERY_RATE_PER_KM);

        // Сохраняем в hidden поля для использования при расчёте
        document.getElementById('deliveryDistanceKm').value = distKm.toFixed(2);
        document.getElementById('deliveryCost').value = deliveryCost;

        // Показываем инфо о доставке
        const infoEl = document.getElementById('deliveryDistanceInfo');
        if (infoEl) {
            infoEl.innerHTML = `
                <span class="material-symbols-outlined" style="font-size:16px; color:#3b82f6;">warehouse</span>
                Ближайший офис: <strong>${nearest.office.name}</strong> — ${distKm.toFixed(1)} км
                &nbsp;|&nbsp;
                <span class="material-symbols-outlined" style="font-size:16px; color:#10b981;">local_shipping</span>
                Доставка: <strong>${deliveryCost.toLocaleString()} ₽</strong>
            `;
            infoEl.style.display = 'flex';
        }

        // Обновляем расчёт цены
        if (typeof updatePriceCalculation === 'function') {
            updatePriceCalculation();
        }
    }
}

// Расстояние по формуле Haversine (км)
function haversineKm(lat1, lng1, lat2, lng2) {
    const R = 6371;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLng = (lng2 - lng1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) ** 2 +
        Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
        Math.sin(dLng / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function findNearestOffice(lat, lng) {
    let nearest = null;
    let minDist = Infinity;
    offices.forEach(function (o) {
        const d = haversineKm(lat, lng, o.latitude, o.longitude);
        if (d < minDist) {
            minDist = d;
            nearest = o;
        }
    });
    return { office: nearest, distanceKm: minDist };
}

async function getAddressByCoords(lat, lng) {
    try {
        const response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&accept-language=ru`);
        const data = await response.json();
        const address = data.display_name || `${lat.toFixed(5)}, ${lng.toFixed(5)}`;
        const addrEl = document.getElementById('deliveryAddress');
        if (addrEl) {
            addrEl.innerHTML = address;
            addrEl.closest('.delivery-address-info').style.display = 'block';
        }
    } catch {
        const addrEl = document.getElementById('deliveryAddress');
        if (addrEl) {
            addrEl.innerHTML = `${lat.toFixed(5)}, ${lng.toFixed(5)}`;
            addrEl.closest('.delivery-address-info').style.display = 'block';
        }
    }
}

function addSearchControl() {
    const searchControl = new ymaps.control.SearchControl({
        options: {
            provider: 'yandex#search',
            noPlacemark: true,
            resultsPerPage: 5
        }
    });

    searchControl.events.add('resultselect', function (e) {
        const index = e.get('index');
        searchControl.getResult(index).then(function (res) {
            const coords = res.geometry.getCoordinates();
            setDeliveryPoint(coords[0], coords[1]);
            map.setCenter(coords, 15);
        });
    });

    map.controls.add(searchControl, { position: { top: 10, right: 10 } });
}

document.addEventListener('DOMContentLoaded', function () {
    initMap();
});
